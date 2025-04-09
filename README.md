# Nixvale

Nixvale is a modern, secure peer-to-peer messaging application built with .NET MAUI and .NET 8. It provides end-to-end encrypted communication with support for both direct messaging and group chats.

## Features

- 🔒 End-to-end encryption for all messages
- 👥 Support for both direct messaging and group chats
- 🌐 P2P networking with DHT support
- 🕶️ Anonymous mode with Tor support
- 📱 Cross-platform support (Windows, macOS, iOS, Android)
- 🎨 Modern, intuitive user interface
- 🔍 Decentralized user discovery
- 📂 Secure file sharing

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- .NET MAUI workload
- Visual Studio 2022 17.8 or later (recommended) or Visual Studio Code

### Building from Source

1. Clone the repository:
```bash
git clone https://github.com/cogrow4/NixVale.git
cd NixVale
```

2. Build the solution:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run --project NixvaleNew.Maui
```

## Project Structure

- `Nixvale.Core` - Core library containing the messaging and networking logic
- `Nixvale.Protocol` - Protocol definitions and network message handling
- `NixvaleNew.Maui` - Cross-platform UI application

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details. This means you are free to use, modify, and distribute this software, but any derivative works must also be distributed under the same license terms.
