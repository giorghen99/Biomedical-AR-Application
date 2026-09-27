import xml.etree.ElementTree as ET
from collections import defaultdict
from datetime import datetime

# Carica il file XML di input
tree = ET.parse("dati_exercise_time.xml")
root = tree.getroot()

# Raccogli i valori per tipo, data e ora
values_by_type_date_hour = defaultdict(list)

for record in root.findall("Record"):
    tipo = record.attrib.get("type")
    valore = record.attrib.get("value")
    start = record.attrib.get("startDate")

    if valore and start:
        try:
            dt = datetime.strptime(start.split(" +")[0], "%Y-%m-%d %H:%M:%S")
            giorno = dt.date().isoformat()   # es. "2025-05-27"
            ora = dt.hour                   # es. 14
            key = (tipo, giorno, ora)
            values_by_type_date_hour[key].append(float(valore))
        except Exception:
            continue

# Crea l'XML manualmente con una riga per media
xml_lines = ['<?xml version="1.0" encoding="utf-8"?>', '<WalkingAverages>']

for (tipo, giorno, ora), valori in sorted(values_by_type_date_hour.items()):
    media = sum(valori) / len(valori)
    riga = f'  <Average type="{tipo}" date="{giorno}" hour="{ora:02d}" value="{media:.2f}" />'
    xml_lines.append(riga)

xml_lines.append('</WalkingAverages>')

# Scrivi il file formattato
with open("exercise_medie_per_ora_giorno.xml", "w", encoding="utf-8") as f:
    f.write("\n".join(xml_lines) + "\n")
