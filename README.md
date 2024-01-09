# Random Team Assigner By NFL Week

## Requirements
- The NFL consists of 32 teams
- The NFL season consists of 18 weeks
- ESPN api exists and has not changed since development and testing in Jan. 2024
  - Endpoints described here: https://gist.github.com/nntrn/ee26cb2a0716de0947a0a4e9a157bc1c
  - Base API: https://sports.core.api.espn.com/v2/sports/football/leagues/nfl
- Must have either 16 (2 teams each) or 32 (1 team each) players configured in the appsettings.json file
  - For even distribution of byes throughout the year 
- A Gmail account with an "app password" saved in the appsettings.json file to send SMTP email through

## Yearly Changes
- Update any changed players details in the appsettings.json file <br />
  (set in an Azure DevOps Library Variable Group for my implementation)
  - Email
  - Name

## Usage
- Execute the .exe weekly after Monday and before game(s) on Thursday <br />
  (scheduled as an Azure DevOps Release at 11:00 AM CST in my implementation)
- Week Override
  - To rerun a previous week set the value of weekoverride property on the appsettings.json file