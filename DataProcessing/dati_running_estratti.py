import xml.etree.ElementTree as ET

# Carica il file XML completo
tree = ET.parse("dati_esportati.xml")
root = tree.getroot()

# Tipi che vogliamo estrarre
included_types = {
    "HKQuantityTypeIdentifierDistanceWalkingRunning",
    "HKQuantityTypeIdentifierRunningPower",
    "HKQuantityTypeIdentifierRunningSpeed",
    "HKQuantityTypeIdentifierRunningGroundContactTime",
    "HKQuantityTypeIdentifierRunningStrideLength",  
    "HKQuantityTypeIdentifierAppleWalkingSteadiness"
}

# Crea nuovo file XML
new_root = ET.Element("HealthData")

for record in root.findall("Record"):
    if record.attrib.get("type") in included_types:
        new_root.append(record)

# Salva in un nuovo file XML
new_tree = ET.ElementTree(new_root)
new_tree.write("dati_running_estratti.xml", encoding="utf-8", xml_declaration=True)
