Instructions for launching project:
Follow the following steps
Install the .NET 9 SDK — download from https://dotnet.microsoft.com/download
Install Git — https://git-scm.com/downloads
git clone https://github.com/JCThree9/SimpleEngine2D
cd SimpleEngine2D
dotnet restore
dotnet new tool-manifest
dotnet tool install dotnet-mgcb
dotnet tool install dotnet-mgcb-editor
dotnet build
To run:dotnet run --project ProjectHub
