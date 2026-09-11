# Random Team Assigner By NFL Week

## Requirements
- .NET 10 SDK (`net10.0`) to build and run locally
- The NFL consists of 32 teams
- The NFL season consists of 18 weeks
- ESPN api exists and has not changed since development and testing in Jan. 2024
  - Endpoints described here: https://gist.github.com/nntrn/ee26cb2a0716de0947a0a4e9a157bc1c
  - Base API: https://sports.core.api.espn.com/v2/sports/football/leagues/nfl
- Need a Google Sheets document with a header row for the player name and player email
- Need a the Google Sheets document ID and api json credentials from a Google Service Account
- Must have either 16 (2 teams each) or 32 (1 team each) players configured in a connected Google Sheets doc
  - For even distribution of byes throughout the year
- SMTP account information saved in the appsettings.json file to send email notifications <br />
  (For my implemenation, a gmail account with an "app password")

## Quote Functionality
The application includes a quote feature that displays inspirational quotes in the email notifications when no team scored exactly 33 points in the previous week.

### Quote Sources
The application supports two methods for retrieving quotes:

1. **Static JSON File (Default)**: The application includes a local `quotes.json` file containing over 6,000 inspirational quotes from various authors. This is the primary method and works offline without any external dependencies.

2. **REST API Fallback**: The application can also be configured to call an external REST API to retrieve quotes. This functionality is controlled by the `QuoteAPIURL` setting in `appsettings.json`.

### Quote File Structure
The `quotes.json` file contains an array of quote objects with the following structure:
```json
[
  {
    "text": "The quote text here",
    "author": "Author Name",
    "source": "Optional source URL",
    "tags": "Optional tags"
  }
]
```

### Configuration
- **QuoteAPIURL**: Set this in `appsettings.json` if you want to use a REST API instead of the local file
- The application automatically falls back to the local quotes file if the API call fails
- No additional configuration is required for the local quotes functionality

## Yearly Changes
- Update any changed players details in the Google Sheets doc <br />
  - Email
  - Name

## Usage
- Run weekly after Monday and before Thursday games.
- GitHub Actions (`.github/workflows/weekly.yml`) is scheduled Thursday 17:00 UTC (11:00 AM CST). See [GitHub Actions](#github-actions).
- Locally: `dotnet run --project TeamAssigner/TeamAssigner.csproj` from a directory that can resolve `appsettings.json`, `creds.json`, and `quotes.json` beside the built binary (the app sets its working directory to the publish/output folder).
- Week Override
  - Scheduled runs leave `AppSettings.WeekOverride` empty.
  - To rerun a previous week, use **Actions → Weekly team assigner → Run workflow** and set `week_override`, or set `WeekOverride` in a local `appsettings.json`.

## GitHub Actions

CI (`.github/workflows/ci.yml`) restores, builds, and publishes **Release** on push/PR to `main` and uploads a `team-assigner` artifact. CI does **not** call Google or send email.

The weekly workflow (`.github/workflows/weekly.yml`) rebuilds, writes `creds.json` and a transformed `appsettings.json` from repository secrets, then runs `dotnet TeamAssigner.dll` on `ubuntu-latest` (the app is a portable `Microsoft.NET.Sdk` console, not a Windows-only ASP.NET package).

### Schedule (UTC vs Central)

GitHub Actions cron is UTC only and does not follow DST:

| Chicago clock | UTC |
|---|---|
| 11:00 AM CDT | 16:00 UTC |
| 11:00 AM CST | 17:00 UTC |

The weekly cron is `0 17 * * 4` (Thursday 17:00 UTC) to match the old Azure Release **11:00 AM CST**. During CDT that is 12:00 PM Chicago. The job sets `TZ=America/Chicago` so `DateTime.Now` matches Central Time.

### Required repository secrets

Create these after merge under **Settings → Secrets and variables → Actions**. Do not commit real values (`creds.json` is gitignored; `appsettings.json` in the repo is placeholders only).

| Secret | Replaces (Azure) | Contents |
|---|---|---|
| `CREDS_JSON` | DownloadSecureFile `creds.json` | Full Google service-account JSON file contents |
| `APPSETTINGS_SHEET_ID` | `AppSettings.SheetID` in variable group `team-assigner-config-prod` | Google Sheet ID |
| `EMAIL_FROM` | `EmailSettings.FromEmail` | Gmail address used as SMTP From |
| `EMAIL_PASSWORD` | `EmailSettings.Psswd` | Gmail app password |

Not stored as secrets (defaults applied at run time):

- `AppSettings.KeyFileName` = `creds.json`
- `AppSettings.SheetRange` = `Sheet1`
- `AppSettings.WeekOverride` = empty unless you pass `week_override` on a manual run
- `EmailSettings.SMTPServer` / `SMTPPort` stay as checked-in `smtp.gmail.com` / `587`

### Manual trigger and retiring Azure

1. Add the four secrets above.
2. Run **Actions → Weekly team assigner → Run workflow** once to validate Google Sheets + email.
3. Confirm CI is green on `main`.
4. Disable the Azure Pipelines build (`devops/azure-pipelines.yml`) and the Azure Release (FileTransform@1 is deprecated). Keep that YAML only as a pointer until then.