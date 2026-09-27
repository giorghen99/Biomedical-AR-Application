import xml.etree.ElementTree as ET

# Carica il file XML completo
tree = ET.parse("dati_esportati.xml")
root = tree.getroot()

# Tipi da estrarre (relativi alla camminata)
included_types = {
    "HKQuantityTypeIdentifierDistanceWalkingRunning",
    "HKQuantityTypeIdentifierWalkingSpeed",
    "HKQuantityTypeIdentifierWalkingStepLength",
    "HKQuantityTypeIdentifierWalkingDoubleSupportPercentage",
    "HKQuantityTypeIdentifierWalkingAsymmetryPercentage",
    "HKQuantityTypeIdentifierWalkingHeartRateAverage"
}

# Crea nuovo contenitore XML
new_root = ET.Element("HealthData")

# Filtra i record desiderati
for record in root.findall("Record"):
    if record.attrib.get("type") in included_types:
        new_root.append(record)

# Salva nel nuovo file XML
new_tree = ET.ElementTree(new_root)
new_tree.write("dati_walking_estratti.xml", encoding="utf-8", xml_declaration=True)
