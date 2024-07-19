# Project Analyzer

Project Analyzer is a command-line tool built on .NET Core for analyzing SVN repositories, inspecting .NET versions, and managing NuGet packages.

## Features

- **SVN Repository Analysis**: Analyze multiple SVN repositories to gather project information.
- **.NET Version Detection**: Detect and display the .NET version used in each project.
- **NuGet Package Management**: Retrieve and compare NuGet package versions to identify updates.
- **Configuration**: Configure SVN repositories and NuGet sources via `appsettings.json` and user secrets.

## Usage

1. **Configuration**:
   - Update `appsettings.json` with your SVN repository URLs and NuGet sources.
   - Store sensitive information (like SVN credentials) securely using user secrets.

2. **Running the Tool**:
   - Build and run the tool using `dotnet run` or use the executable directly.
   - Follow the prompts to analyze SVN repositories and view project details.

3. **Output**:
   - View detected .NET versions and NuGet package details for each project.
   - Identify outdated NuGet packages highlighted in red for easy update detection.

## Requirements

- .NET Core SDK (version 8.0 or higher)
- SVN server access (for repository analysis)
- Internet connectivity (for NuGet package version checks)
