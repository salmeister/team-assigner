# Random Team Assigner By NFL Week

## Requirements
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
- Execute the .exe weekly after Monday and before game(s) on Thursday <br />
  (scheduled as an Azure DevOps Release at 11:00 AM CST in my implementation)
- Week Override
  - To rerun a previous week set the value of weekoverride property on the appsettings.json file