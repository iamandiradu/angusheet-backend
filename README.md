# Angusheet - Backend

This project was generated with [.NET](https://github.com/dotnet/sdk)

## Backend (.NET)

### Development server

dotnet run

### Build

#### This way of publishing has been sketchy. If it doesn\'t ork, use the VSCode Azure Plugin.

dotnet publish -c Release -o ./publish

### Deploy

az webapp deploy --resource-group NewResourceGroup --name Angusheet --src-path ./publish --type zip
