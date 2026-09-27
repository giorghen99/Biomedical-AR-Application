import xml.etree.ElementTree as ET
from datetime import datetime
from collections import defaultdict
import statistics

# Carica il file XML originale
tree = ET.parse('heart_rate.xml')  # <-- cambia qui col tuo file originale
root = tree.getroot()

# Dizionario: {(giorno, ora): [battiti]}
dati_hr = defaultdict(list)

for record in root.findall('Record'):
    if record.attrib.get('type') == 'HKQuantityTypeIdentifierHeartRate':
        value = float(record.attrib.get('value', 0))
        start_date = record.attrib.get('startDate')
        if start_date:
            dt = datetime.strptime(start_date, "%Y-%m-%d %H:%M:%S %z")
            chiave = (dt.date(), dt.hour)
            dati_hr[chiave].append(value)

# Crea nuovo albero XML per output
root_output = ET.Element('HeartRateAverages')

for (giorno, ora) in sorted(dati_hr):
    media = statistics.mean(dati_hr[(giorno, ora)])
    entry = ET.SubElement(root_output, 'HeartRateAverage')
    entry.set('date', str(giorno))
    entry.set('hour', f"{ora:02d}")
    entry.set('average', f"{media:.2f}")

# Scrivi su file con linee separate
tree_output = ET.ElementTree(root_output)
ET.indent(tree_output, space="  ", level=0)  # Python 3.9+
tree_output.write('heart_rate_media.xml', encoding='utf-8', xml_declaration=True)
