# Team Assigner

Weekly NFL “two teams each” email assigner. It reads player names and emails from a Google Sheet, picks teams for the current NFL regular-season week (with even bye distribution), and emails everyone their assignments.

## What it does

1. Looks up the current NFL regular season and week from ESPN.
2. Reads 16 or 32 players from Google Sheets.
3. Randomizes two teams per player (or one team each if you have 32), spreading bye teams fairly.
4. Emails the table to every player.
5. For week 2+, appends a “who scored 33 last week?” blurb (or a quote if nobody did). Week 1 skips that section — there is no prior regular-season week.

## Requirements

- **.NET 10 SDK** (`net10.0`) to build and run locally
- **16 or 32 players** (even bye distribution over 18 weeks / 32 NFL teams)
- A **Google Sheet** with player name + email, and a **Google Cloud service account** that can read it
- **SMTP** to send mail (the included workflow uses Gmail + an [app password](https://support.google.com/accounts/answer/185833))
- ESPN public APIs (no ESPN key). See unofficial endpoint notes: https://gist.github.com/nntrn/ee26cb2a0716de0947a0a4e9a157bc1c

## Google Sheet format

`SheetRange` defaults to `Sheet1` (the whole first tab). **Row 1 is a header** and is skipped. Data starts on row 2:

| Name | Email |
| --- | --- |
| Ada Lovelace | ada@example.com |
| Grace Hopper | grace@example.com |

- Column A: player name  
- Column B: email  
- Add **exactly 16 or 32** data rows (plus the header)

Share the sheet with the service account’s client email (Viewer is enough).

## Local setup

1. Clone the repo and restore/build with .NET 10:

   ```bash
   git clone https://github.com/salmeister/team-assigner.git
   cd team-assigner
   dotnet restore TeamAssigner.sln
   ```

2. Put the Google service-account JSON next to the app as **`creds.json`**. That filename is gitignored — do not commit it.

3. Copy or edit `TeamAssigner/appsettings.json` (repo copy is placeholders only):

   | Key | Where | What |
   | --- | --- | --- |
   | `AppSettings.BaseAPIURL` | ESPN core API | Default is fine |
   | `AppSettings.ScoresBaseAPIURL` | ESPN site scoreboard | Default is fine |
   | `AppSettings.SheetID` | Google | Sheet ID from the spreadsheet URL |
   | `AppSettings.SheetRange` | Google | Usually `Sheet1` |
   | `AppSettings.KeyFileName` | local file | `creds.json` |
   | `AppSettings.WeekOverride` | optional | Empty = current week; a number reruns that week |
   | `AppSettings.QuoteAPIURL` | optional | Unused if you rely on the bundled `quotes.json` |
   | `EmailSettings.SMTPServer` / `SMTPPort` | SMTP | `smtp.gmail.com` / `587` |
   | `EmailSettings.FromEmail` | SMTP | From address |
   | `EmailSettings.Psswd` | SMTP | Gmail **app password**, not your login password |

4. Run from a directory that can see `appsettings.json`, `creds.json`, and `quotes.json` beside the built DLL (the app sets its working directory to the output folder):

   ```bash
   dotnet run --project TeamAssigner/TeamAssigner.csproj
   ```

## GitHub Actions

| Workflow | When | What |
| --- | --- | --- |
| **CI** (`.github/workflows/ci.yml`) | push/PR to `main` | Restore, Release build, publish, upload `team-assigner` artifact. No Google, no email. |
| **Weekly team assigner** (`.github/workflows/weekly.yml`) | Thursday cron + manual | Rebuilds, writes `creds.json` and `appsettings.json` from secrets, runs `dotnet TeamAssigner.dll`. |

### Schedule

GitHub cron is UTC only (`0 17 * * 4` = Thursday 17:00 UTC = **11:00 AM CST**). During CDT that is 12:00 PM Chicago. The job sets `TZ=America/Chicago` so `DateTime.Now` is Central Time.

### Required Actions secrets

**Settings → Secrets and variables → Actions**. Never commit these.

| Secret | Contents |
| --- | --- |
| `CREDS_JSON` | Full Google service-account JSON |
| `APPSETTINGS_SHEET_ID` | Google Sheet ID |
| `EMAIL_FROM` | SMTP From / Gmail address |
| `EMAIL_PASSWORD` | Gmail app password |

Applied at run time (not secrets): `KeyFileName=creds.json`, `SheetRange=Sheet1`, SMTP `smtp.gmail.com:587`. `WeekOverride` is empty unless you pass `week_override` on a manual run.

### Re-run the weekly job

1. Open **Actions → Weekly team assigner → Run workflow**.
2. Leave `week_override` blank to use the current NFL week, or set it to a number (`1`–`18`) to force that week.
3. Confirm the run succeeds and the assignment email arrives.

Same override locally: set `AppSettings.WeekOverride` in `appsettings.json`.

## ESPN scoreboard (403 / User-Agent / week 1)

`sports.core.api.espn.com` (season/weeks/teams) is what assignment uses. The **previous-week 33-point blurb** uses `site.api.espn.com` scoreboard, which Akamai often **403s** from GitHub-hosted runner IPs when `HttpClient` sends **no User-Agent**.

`RESTUtil` uses one shared `HttpClient` and always sends a User-Agent (Get and Put), plus `Accept` / `Accept-Language`. If a request still returns 403, it retries a couple of alternate User-Agents (app-style first, then a browser UA).

- **Week 1:** previous-week fetch is skipped (no `week=0` scoreboard call). The assignment email still sends; the last-week/quote section is omitted.
- **Week 2+:** a scoreboard 403 is **non-fatal**. The job logs a warning, notes that last-week scores were unavailable, and still sends the assignment email.

Quick check that a User-Agent is required (curl’s default UA is *not* empty — use an empty header to mimic GitHub `HttpClient`):

```bash
SCOREBOARD='https://site.api.espn.com/apis/site/v2/sports/football/nfl/scoreboard?dates=2026&seasontype=2&week=1'

# Often 403 — same missing-UA behavior as default HttpClient
curl -sS -o /dev/null -w "%{http_code}\n" -H "User-Agent:" "$SCOREBOARD"

# Same User-Agent RESTUtil sends first
curl -sS -o /dev/null -w "%{http_code}\n" \
  -A "Mozilla/5.0 (compatible; TeamAssigner/1.0; +https://github.com/salmeister/team-assigner)" \
  -H "Accept: application/json, text/plain, */*" \
  "$SCOREBOARD"
```

## Yearly maintenance

- Update names and emails in the Google Sheet before week 1.
- Keep 16 or 32 players.
- Confirm GitHub secrets still match the sheet and Gmail app password.

## Quotes

If nobody scored exactly 33 the previous week, the email can include a random line from the bundled `quotes.json` (thousands of quotes; works offline). `QuoteAPIURL` is optional and unused unless you point it at an API.
