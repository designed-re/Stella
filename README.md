# server

Stella is an **unofficial implementation of Konami's e-amusement server**.  

## Features
- Partial implementation of e-amusement protocol
- Data exchange between client and server
- Modular and extensible architecture
- Plugin support

## Currently Enabled Plugins
- Core Plugin
- KFC Plugin

## Requirements
- .NET SDK 10
- Supported OS: Windows, Linux
- MariaDB

## Installation & Usage
```bash
# Clone the repository
git clone https://gitlab.lunalight.place/stella/server.git
cd server

# Build the project
dotnet build

# Run the server
dotnet run
```