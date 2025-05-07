# Coinspot Market Scraper

## 📌 Project Overview
Coinspot Market Scraper is a robust, scalable, and feature-rich API for cryptocurrency analysis. It allows users to pull, analyze, and store market data directly from CoinSpot, and provides advanced technical analysis tools like RSI, MACD, Momentum, EMA, and more. This API is designed to be fast, reliable, and easy to use for both developers and analysts.

## 🚀 Features
- Fetches real-time coin data from CoinSpot.
- Stores all data in a local SQLite database (`marketdata.db`).
- Provides advanced analysis tools:
  - RSI (Relative Strength Index)
  - MACD (Moving Average Convergence Divergence)
  - Momentum
  - Volatility
  - EMA (Exponential Moving Average)
  - Spread calculation
- Automatic data collection every 5 minutes via `CoinSnapshotCollector`.
- Fully documented API with Swagger UI.
- Highly efficient using Write-Ahead Logging (WAL) for SQLite.

## 🏗️ Architecture
- **SQLite Database** (`marketdata.db`): Local storage of coin price history.
- **CoinSnapshotCollector** (Services): Background service that collects and stores data every 5 minutes.
- **ToolsController** (Controllers): API endpoints for advanced technical analysis.
- **CoinspotScraper** (Services): Fetches live coin data from CoinSpot.
- **CoinStrengthAnalyzer** (Services): Core service for all analysis calculations.
- **MarketDbContext** (Services): Manages database access.

## 📑 API Endpoints
### Data Collection
- **GET /api/history** - Get all historical snapshots.
- **GET /api/history/{coin}** - Get historical data for a specific coin.

### Analysis Tools
- **GET /api/tools/rsi/{coin}** - Calculate RSI for a coin.
- **GET /api/tools/macd/{coin}** - Calculate MACD for a coin.
- **GET /api/tools/momentum/{coin}** - Calculate Momentum.
- **GET /api/tools/volatility/{coin}** - Calculate Volatility.
- **GET /api/tools/spread/{coin}** - Calculate Spread.
- **GET /api/tools/ema/{coin}?period=20** - Calculate EMA (customizable period).
- **GET /api/tools/evaluate/{coin}** - Full technical analysis report.

## ⚡ Setup and Installation
### Prerequisites
- .NET 6.0 or higher
- Visual Studio or VS Code
- Docker (optional for containerized deployment)

### Running Locally
1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/coinspot-market-scraper.git
   cd coinspot-market-scraper
