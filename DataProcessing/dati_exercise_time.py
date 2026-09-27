import xml.etree.ElementTree as ET

# Carica il file XML
tree = ET.parse("dati_esportati.xml")
root = tree.getroot()

# Tipo specifico da estrarre
included_type = "HKQuantityTypeIdentifierAppleExerciseTime"

# Crea un nuovo contenitore XML
new_root = ET.Element("HealthData")

# Aggiungi solo i record con il tipo desiderato
for record in root.findall("Record"):
    if record.attrib.get("type") == included_type:
        new_root.append(record)

# Salva nel nuovo file
new_tree = ET.ElementTree(new_root)
new_tree.write("dati_exercise_time.xml", encoding="utf-8", xml_declaration=True)
