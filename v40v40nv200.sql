Select StockMaster.Symbol, StockMaster.Close, V20_CANDLE_STRATEGY.FROM_DATE, V20_CANDLE_STRATEGY.TO_DATE, V20_CANDLE_STRATEGY.DIFF_PCT, V20_CANDLE_STRATEGY.BUY_PRICE, V20_CANDLE_STRATEGY.SELL_PRICE from V20_CANDLE_STRATEGY
INNER JOIN  StockMaster
ON StockMaster.StockMasterID = V20_CANDLE_STRATEGY.StockMasterID
WHERE (V20_CANDLE_STRATEGY.BUY_PRICE - StockMaster.Close) > 0 AND (V20_CANDLE_STRATEGY.BUY_PRICE - StockMaster.Close) < 5

/*
Update StockMaster set V40 = 0, V40N = 0, V200 = 0
Update StockMaster set V40 = 1 WHERE

Symbol =  'HDFCBANK' OR Symbol =  'ICICIBANK'
OR Symbol =  'KOTAKBANK' OR Symbol =  'HDFC'  OR Symbol =  'BAJFINANCE'  OR Symbol =  'ICICIGI'  OR Symbol =  'HDFCLIFE'  
OR Symbol =  'BAJAJFINSV'  OR Symbol =  'ICICIPRULI'  OR Symbol =  'HDFCAMC'  OR Symbol =  'NAM_INDIA'  OR Symbol =  'PGHH'  
OR Symbol =  'MARICO'  OR Symbol =  'COLPAL'  OR Symbol =  'HINDUNILVR'  OR Symbol =  'BAJAJHLDNG'  OR Symbol =  'DABUR'  
OR Symbol =  'NESTLEIND'  OR Symbol =  'ITC'  OR Symbol =  'ASIANPAINT'  
OR Symbol =  'BERGEPAINT' OR Symbol =  'AKZOINDIA'  OR Symbol ='TITAN'  OR Symbol =  'BATAINDIA'  OR Symbol =  'PAGEIND'  
OR Symbol =  'WHIRLPOOL'  OR Symbol =  'HAVELLS'  OR Symbol =  'TCS'  OR Symbol =  'INFY'  OR Symbol =  'HCLTECH'  
OR Symbol =  'ABBOTINDIA'  OR Symbol =  'GLAXO'  OR Symbol =  'SANOFI'  OR Symbol =  'PFIZER'  OR Symbol =  'PIDILITIND'  
OR Symbol =  'GILLETTE'  OR Symbol =  'NIFTYBEES'  OR Symbol =  'BANKBEES'  OR Symbol =  'AXISBANK'  
OR Symbol =  'BAJAJ_AUTO'

Update StockMaster set V40N = 1 WHERE
Symbol = '3MINDIA' OR Symbol = 'ASTRAZEN' OR Symbol = 'AVANTIFEED' OR Symbol = 'BAYERCROP' OR Symbol = 'BOSCHLTD' 
OR Symbol = 'CAPLIPOINT' OR Symbol = 'CERA' OR Symbol = 'DIXON' OR Symbol = 'EICHERMOT' OR Symbol = 'EQUITAS' 
OR Symbol = 'EQUITASBNK' OR Symbol = 'ERIS' OR Symbol = 'FINCABLES' OR Symbol = 'FINEORG' OR Symbol = 'GODREJCP' 
OR Symbol = 'HONAUT' OR Symbol = 'ISEC' OR Symbol = 'JCHAC' OR Symbol = 'KANSAINER' OR Symbol = 'LALPATHLAB' 
OR Symbol = 'MCDOWELL_N' OR Symbol = 'MCX' OR Symbol = 'MOTILALOFS' OR Symbol = 'OFSS' OR Symbol = 'POLYCAB' 
OR Symbol = 'RADICO' OR Symbol = 'RAJESHEXPO' OR Symbol = 'RELAXO' OR Symbol = 'SFL' OR Symbol = 'SIS' OR Symbol = 'SUNTV' 
OR Symbol = 'SYMPHONY' OR Symbol = 'TASTYBITE' OR Symbol = 'TEAMLEASE' OR Symbol = 'TTKPRESTIG' OR Symbol = 'UJJIVAN' 
OR Symbol = 'UJJIVANSFB' OR Symbol = 'VGUARD' OR Symbol = 'VINATIORGA' OR Symbol = 'VIPIND'


Update StockMaster set V200 = 1 WHERE

Symbol = 'TCS' OR
Symbol = 'INFY' OR
Symbol = 'HINDUNILVR' OR
Symbol = 'ITC' OR
Symbol = 'ASIANPAINT' OR
Symbol = 'HCLTECH' OR
Symbol = 'NESTLEIND' OR
Symbol = 'COALINDIA' OR
Symbol = 'PIDILITIND' OR
Symbol = 'HINDZINC' OR
Symbol = 'BAJAJ_AUTO' OR
Symbol = 'TECHM' OR
Symbol = 'DABUR' OR
Symbol = 'DIVISLAB' OR
Symbol = 'AMBUJACEM' OR
Symbol = 'HAVELLS' OR
Symbol = 'LTI' OR
Symbol = 'HAL' OR
Symbol = 'BEL' OR
Symbol = 'MARICO' OR
Symbol = 'GAIL' OR
Symbol = 'MCDOWELL_N' OR
Symbol = 'IRCTC' OR
Symbol = 'SCHAEFFLER' OR
Symbol = 'NAUKRI' OR
Symbol = 'TATAELXSI' OR
Symbol = 'PAGEIND' OR
Symbol = 'MINDTREE' OR
Symbol = 'ASTRAL' OR
Symbol = 'PGHH' OR
Symbol = 'COLPAL' OR
Symbol = 'HDFCAMC' OR
Symbol = 'MPHASIS' OR
Symbol = 'LTTS' OR
Symbol = 'GLAND' OR
Symbol = 'ABBOTINDIA' OR
Symbol = 'POLYCAB' OR
Symbol = 'NMDC' OR
Symbol = 'GUJGASLTD' OR
Symbol = 'MANYAVAR' OR
Symbol = 'CUMMINSIND' OR
Symbol = 'SONACOMS' OR
Symbol = 'COROMANDEL' OR
Symbol = 'IGL' OR
Symbol = 'DEEPAKNTR' OR
Symbol = 'OFSS' OR
Symbol = 'SUPREMEIND' OR
Symbol = 'PERSISTENT' OR
Symbol = 'GRINDWELL' OR
Symbol = 'TIMKEN' OR
Symbol = 'SUMICHEM' OR
Symbol = 'SKFINDIA' OR
Symbol = 'GLAXO' OR
Symbol = 'VINATIORGA' OR
Symbol = 'BAYERCROP' OR
Symbol = 'CRISIL' OR
Symbol = 'EMAMILTD' OR
Symbol = 'IPCALAB' OR
Symbol = 'COFORGE' OR
Symbol = 'CLEAN' OR
Symbol = 'SUNTV' OR
Symbol = 'FINEORG' OR
Symbol = 'PFIZER' OR
Symbol = 'NAM_INDIA' OR
Symbol = 'KAJARIACER' OR
Symbol = 'AJANTPHARM' OR
Symbol = 'GILLETTE' OR
Symbol = 'AFFLE' OR
Symbol = 'CARBORUNIV' OR
Symbol = 'KPITTECH' OR
Symbol = 'ALKYLAMINE' OR
Symbol = 'CENTURYPLY' OR
Symbol = 'BDL' OR
Symbol = 'NATIONALUM' OR
Symbol = 'INDIAMART' OR
Symbol = 'EXIDEIND' OR
Symbol = 'CDSL' OR
Symbol = 'GSPL' OR
Symbol = 'IEX' OR
Symbol = 'JBCHEPHARM' OR
Symbol = 'SANOFI' OR
Symbol = 'KEI' OR
Symbol = 'BASF' OR
Symbol = 'TTKPRESTIG' OR
Symbol = 'ABSLAMC' OR
Symbol = 'SUVENPHAR' OR
Symbol = 'CAMS' OR
Symbol = 'TANLA' OR
Symbol = 'REDINGTON' OR
Symbol = 'BALAMINES' OR
Symbol = 'GNFC' OR
Symbol = 'CASTROLIND' OR
Symbol = 'VGUARD' OR
Symbol = 'KIMS' OR
Symbol = 'RHIM' OR
Symbol = 'EIDPARRY' OR
Symbol = 'ERIS' OR
Symbol = 'CYIENT' OR
Symbol = 'FINPIPE' OR
Symbol = 'BSOFT' OR
Symbol = 'AKZOINDIA' OR
Symbol = 'LXCHEM' OR
Symbol = 'MGL' OR
Symbol = 'MAZDOCK' OR
Symbol = 'INTELLECT' OR
Symbol = 'BCG' OR
Symbol = 'SONATSOFTW' OR
Symbol = 'SHYAMMETL' OR
Symbol = 'JUBLINGREA' OR
Symbol = 'ECLERX' OR
Symbol = 'SPLPETRO' OR
Symbol = 'FINCABLES' OR
Symbol = 'RITES' OR
Symbol = 'RTNINDIA' OR
Symbol = 'TRITURBINE' OR
Symbol = 'PRINCEPIPE' OR
Symbol = 'NBCC' OR
Symbol = 'GAEL' OR
Symbol = 'VAIBHAVGBL' OR
Symbol = 'HGS' OR
Symbol = 'CAPLIPOINT' OR
Symbol = 'MASTEK' OR
Symbol = 'LUXIND' OR
Symbol = 'TCI' OR
Symbol = 'GLS' OR
Symbol = 'ZENSARTECH' OR
Symbol = 'VSTIND' OR
Symbol = 'SHARDACROP' OR
Symbol = 'APARINDS' OR
Symbol = 'HEIDELBERG' OR
Symbol = 'GPIL' OR
Symbol = 'CMSINFO' OR
Symbol = 'MOIL' OR
Symbol = 'DHANUKA' OR
Symbol = 'TINPLATE' OR
Symbol = 'MAITHANALL' OR
Symbol = 'ORIENTCEM' OR
Symbol = 'TATAMETALI' OR
Symbol = 'TIRUMALCHM' OR
Symbol = 'SOTL' OR
Symbol = 'LGBBROSLTD' OR
Symbol = 'SIYSIL' OR
Symbol = 'BEPL' OR
Symbol = 'ANDHRSUGAR' OR
Symbol = 'GTPL' OR
Symbol = 'PANAMAPET' OR
Symbol = 'IGPL' OR
Symbol = 'MANALIPETC' OR
Symbol = 'INEOSSTYRO' OR
Symbol = 'EKC' OR
Symbol = 'DVL' OR
Symbol = 'NAHARCAP' OR
Symbol = 'UNIVPHOTO' OR
Symbol = 'DHUNINV' OR
Symbol = 'JINDALPHOT' OR
Symbol = 'HDFCBANK' OR
Symbol = 'AUBANK' OR
Symbol = 'ICICIBANK' OR
Symbol = 'KOTAKBANK' OR
Symbol = 'AXISBANK' OR
Symbol = 'SBIN' OR
Symbol = 'FEDERALBNK' OR
Symbol = 'INDUSINDBK' OR
Symbol = 'ISEC' OR
Symbol = 'JPOLYINVST' OR
Symbol = 'HDFCAMC' OR
Symbol = 'MOTILALOFS' OR
Symbol = 'MUTHOOTFIN' OR
Symbol = 'SBICARD' OR
Symbol = 'RECLTD' OR
Symbol = 'PFC' OR
Symbol = 'CHOLAFIN' OR
Symbol = 'IIFL' OR
Symbol = 'BAJFINANCE' OR
Symbol = 'CHOLAHLDNG' OR
Symbol = 'MANAPPURAM' OR
Symbol = 'IRFC' OR
Symbol = 'SUNDARMFIN' OR
Symbol = 'HDFC' OR
Symbol = 'SHRIRAMCIT' OR
Symbol = 'HUDCO' OR
Symbol = 'BAJAJFINSV' OR
Symbol = 'SRTRANSFIN' OR
Symbol = 'ABCAPITAL' OR
Symbol = 'BAJAJHLDNG' OR
Symbol = 'LICHSGFIN'
*/


