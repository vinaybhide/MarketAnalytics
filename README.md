# MarketAnalytics
Stock Strategy implementation
<h3>Summary</h3>
  Project idea is use NSE listed companies (WIP for other global exchanges) for:
  <ol>
  <li>Store all companies along with their CMP</li>
  <li>Store 10 years price data</li>
  <li>Show SMA chart for 200, 50 & 20 periods</li>
  <li>Show RSI chart for period value 14</li>
  <li>Show Stochastics chart for fastk = 20 and slowd = 20</li>
  <li>Backtest for buy sell algorithms (data table & chart)
    <ul>
      <li>Show buy indicator when price is below SMA(200/50/20)</li>
      <li>Show sell indicator when price is above SMA (20/50/200)</li>
      <li>Identify bullish engulfing pattern (uptrend) and indicate buy & sell opportunities</li>
      <li>Identify bearish engulfing pattern (downtrend) and indicate buy & sell opportunities</li>
      <li>Identify uptrend with continuous green candle of at least 20% and show buy & sell opportunities</li>
      <li>Identify a price date when stock price hit below 67% than lifetime highest close & for that stock check if the CMP is about 50% lower than 52 week highese close for buy opportunity</li>
    </ul>
  </li>
  <li>Portfolio
    <ul>
      <li>Create portfolios</li>
      <li>Add transactions</li>
      <li>Consolidate portfolio valuation</li>
      <li>Individual transaction valuation</li>
    </ul>
  </li>
  </ol>
  <h3>Technologies Used</h3>
  <ol>
  <li>ASP.Net Core 3.0 (Code first) with Razor</li>
  <li>Entity framework core (EF core) 6.0</li>
  <li>Google charts</li>
  <li>Java script</li>
  <li>SQLite</li>
</ol>
