# .NET BuildContainers - Simplified Containerization

Welcome to .NET BuildContainers! This repository helps you create containerized versions of your applications seamlessly using `dotnet publish`. Thanks to the .NET SDK's support for container images as an output type, the process has never been easier. 

You can learn more about the .NET SDK's built-in containerization support in the official [announcement](https://devblogs.microsoft.com/dotnet/announcing-builtin-container-support-for-the-dotnet-sdk/).

## Getting Started

If you are interested in a straightforward setup for minimal API, check out the [introduction templates](https://github.com/Johnz86/DotnetMinimalApi).

Here's how to get your project up and running:

1. Create a new minimal API project with authorization:

```bash
dotnet new miniapiauth -o BuildContainersFeature
```

2. Add the Microsoft.NET.Build.Containers package:

```bash
dotnet add package Microsoft.NET.Build.Containers
```

3. Publish your project for linux-x64:

```bash
dotnet publish --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer
```

4. Run your app using the newly created container:

```bash
docker run -it --rm -p 5010:80 buildcontainersfeature:1.0.0
```

## Working with Secrets and Certificates in .NET

Managing secrets in .NET can be a challenge, but these guides on [storing secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-7.0&tabs=windows) and creating a [self-signed certificate](https://learn.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide) are handy resources.

Execute the following commands to setup https:

Note replace $CREDENTIAL_PLACEHOLDER with your password:

```bash
dotnet dev-certs https -ep "$env:USERPROFILE\.aspnet\https\buildcontainersfeature.pfx" -p $CREDENTIAL_PLACEHOLDER
dotnet dev-certs https --trust
dotnet user-secrets init
dotnet user-secrets set "Kestrel:Certificates:Development:Password" $CREDENTIAL_PLACEHOLDER --project .\BuildContainersFeature.csproj
$secretValue = dotnet user-secrets list | Select-String 'Kestrel:Certificates:Development:Password' | ForEach-Object { $_.ToString().Split(' ')[2] }
```

Then, run your app in a container with HTTPS:

```bash
docker run --rm -it -p 8000:80 -p 8001:443 -v $env:USERPROFILE\.aspnet\https\:/https/ -e ASPNETCORE_URLS="https://+:443;http://+:80" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_Kestrel__Certificates__Default__Password=$secretValue -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/buildcontainersfeature.pfx buildcontainersfeature:1.0.0
```

## Conclusion

With this repository, containerizing your .NET applications becomes a breeze. Feel free to contribute, provide feedback, and share your thoughts about the project. Happy coding!
