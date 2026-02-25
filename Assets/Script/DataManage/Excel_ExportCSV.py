import pandas as pd
from pathlib import Path
import re
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')

if len(sys.argv) < 3:
    print("Error: 引数が足りません。Excelのパスと出力先のパスが必要です。", file=sys.stderr)
    sys.exit(1)

playerSelect = str(sys.argv[1])  
outputDir = str(sys.argv[2])    

def sanitize_filename(name: str) -> str:
    return re.sub(r'[\\/:*?"<>|]+', '_', str(name)).strip() or "sheet"

def excel_to_csv_all_sheets(excel_path: str, out_dir: str, encoding="utf-8-sig"):
    excel_path = Path(excel_path).expanduser().resolve()
    out_dir = Path(out_dir).expanduser().resolve()

    out_dir.mkdir(parents = True, exist_ok = True)

    try:
        # header=1 は「1行目を無視して2行目をヘッダーにする」設定です。
        # 1行目からヘッダーの場合は header=0 に変更してください。
        sheets = pd.read_excel(excel_path, sheet_name = None, header = 1)
    except Exception as e:
        print(f"Excelの読み込みエラー: {e}", file=sys.stderr)
        sys.exit(1)

    for sheet_name, df in sheets.items():
        df = df.dropna(axis = 1, how = "all")  
        df = df.dropna(how = "all")    

        csv_path = out_dir / (sanitize_filename(sheet_name) + ".csv")
        df.to_csv(csv_path, index=False, encoding=encoding)

    print(f"Input : {excel_path}")
    print(f"Output: {out_dir}  (exported {len(sheets)} sheets)")

# 実行
excel_to_csv_all_sheets(playerSelect, outputDir)