# AWSLab
Staff related to AWS learning

###### Prerequisites:
1. Setup AWS account and generate access key - https://docs.aws.amazon.com/IAM/latest/UserGuide/id_credentials_access-keys.html#Using_CreateAccessKey
2. Install and configure AWS CLI - https://docs.aws.amazon.com/cli/latest/userguide/cli-chap-install.html
3. Install .NET 5 SDK - https://dotnet.microsoft.com/download/dotnet/5.0

###### Setup:
Change const in Program.cs (bucket name rules - https://docs.aws.amazon.com/AmazonS3/latest/userguide/bucketnamingrules.html)
   
   **Note** - _upload and download folders should be inside /bin/<BuildConfig>/net5.0/ directory of S3BasicOperation project_
   
   _Upload files should be in upload folder_

###### Usage:

**Run from command line**

Set environment variables

Windows:

`set AWS_PROFILE=default`

`set AWS_REGION=eu-central-1`

MacOS/Linux:

`AWS_PROFILE='default'`

`AWS_REGION='eu-central-1'`

Build and run:

`dotnet build --project S3BasicOperations/S3BasicOperations.csproj`

`cd S3BasicOperations/bin/Debug/net5.0`

`dotnet run S3BasicOperations.dll`

**Run from IDE (VS, Rider):**

Set AWS_ env parameters inside launchSettings.json:

`"environmentVariables": {`

`"AWS_PROFILE": "default",`

`"AWS_REGION": "eu-central-1"`

`}`

Run S3BasicOperations project
