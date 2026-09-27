\# 🫀 Biomedical AR Application



> An Augmented Reality application developed in Unity for the visualization and exploration of biomedical and physical activity data.



🇬🇧 \[English](#-english) | 🇮🇹 \[Italiano](#-italiano)



\---



\# 🇬🇧 English



\## 📖 Overview



\*\*Biomedical AR Application\*\* is an Augmented Reality project developed with \*\*Unity, C#, Vuforia and Python\*\* during a university internship.



The application combines an interactive \*\*3D model of the heart and lungs\*\* with biomedical and physical activity data. Through an AR target, the user can access the 3D visualization and explore heart-rate information associated with different dates and hours of the day.



A dedicated 2D dashboard allows the user to select a date, inspect hourly heart-rate information, view daily statistics and examine exercise, running and walking data.



Python scripts are used as a preprocessing layer to filter and aggregate the source XML datasets before they are loaded by the Unity application.



\---



\## ✨ Main Features



\- 📱 \*\*Augmented Reality visualization\*\* using Vuforia

\- 🫀 Interactive \*\*3D heart and lungs model\*\*

\- ❤️ Visualization of heart-rate data (BPM)

\- 🕐 \*\*24-hour heart-rate dashboard\*\*

\- 📅 Date selection by year, month and day

\- 📊 Daily minimum, maximum and average BPM visualization

\- 🔎 Detailed information for individual hours

\- 🏃 Exercise and running information

\- 🚶 Walking information

\- 🎨 Color-coded BPM indicators

\- 🔄 Transition between the 2D dashboard and AR visualization

\- 🐍 Python-based XML data preprocessing

\- 📱 Android-oriented implementation



\---



\## 🎨 BPM Color Coding



The application uses visual indicators to make heart-rate information immediately understandable.



| Color | Meaning |

|---|---|

| 🟡 Yellow | BPM below 60 |

| 🟢 Green | BPM between 60 and 100 |

| 🔴 Red | BPM above 100 |

| ⚫ Gray | No data available |



These thresholds are used as visualization rules inside the application and should not be interpreted as medical diagnosis.



\---



\## 📊 Heart Rate Dashboard



The main dashboard is dynamically generated using \*\*Unity UI Toolkit\*\*.



The user can select:



\- Year

\- Month

\- Day



After selecting a valid date, the application filters the available records and generates a visualization containing \*\*24 indicators\*\*, one for each hour of the day.



Each indicator changes color according to the average BPM recorded for that hour.



Selecting an hour opens a detailed panel containing, when available:



\- Average BPM

\- Minimum BPM

\- Maximum BPM

\- Exercise time

\- Running distance

\- Walking distance



The selected heart-rate information can also be displayed directly in the AR view.



\---



\## 🥽 Augmented Reality



AR functionality is implemented using \*\*Vuforia Engine\*\*.



When the configured image target is detected, the application displays the 3D biomedical model and provides access to the AR interface.



The application listens to Vuforia target tracking events and manages the visibility of the AR interface accordingly.



Users can switch between:



\*\*AR View → Detailed Dashboard → AR Statistics\*\*



Heart-rate statistics for the selected date or hour can therefore be associated with the augmented 3D visualization.



\---



\## 🐍 Data Processing



A set of Python scripts is included in the `DataProcessing` directory.



These scripts preprocess XML records before they are used by Unity.



The processing pipeline extracts and aggregates information related to:



\- Heart rate

\- Exercise time

\- Running

\- Walking



Records are grouped by \*\*date and hour\*\*, producing smaller XML datasets suitable for the Unity application.



\### Processing Pipeline



```text

Source XML data

&#x20;     │

&#x20;     ▼

Python filtering scripts

&#x20;     │

&#x20;     ├── Heart Rate

&#x20;     ├── Exercise

&#x20;     ├── Running

&#x20;     └── Walking

&#x20;     │

&#x20;     ▼

Hourly aggregation

&#x20;     │

&#x20;     ▼

Processed XML files

&#x20;     │

&#x20;     ▼

Unity StreamingAssets

&#x20;     │

&#x20;     ▼

DynamicUIManager

&#x20;     │

&#x20;     ▼

2D Dashboard + AR Visualization

```



The XML files used by the application are loaded at runtime from Unity's `StreamingAssets` directory.



For platform compatibility, the project supports both direct file access and `UnityWebRequest` when loading these resources.



\---



\## 🛠️ Technologies



| Technology | Purpose |

|---|---|

| Unity | Application development and real-time 3D |

| C# | Application logic and UI management |

| Vuforia Engine | Augmented Reality and image-target tracking |

| Unity UI Toolkit | Dynamic user interface |

| TextMeshPro | AR text rendering |

| Python | Biomedical data preprocessing |

| XML | Processed data storage |

| Android | Target mobile platform |



\---



\## 🧩 Main C# Components



\### `DynamicUIManager.cs`



Coordinates the main application interface and data flow.



Responsibilities include:



\- Loading XML datasets

\- Date filtering

\- Daily BPM calculation

\- Dynamic UI generation

\- Communication with the heart-rate chart

\- Vuforia target event handling

\- Switching between dashboard and AR visualization



\### `HeartRateChart.cs`



Implements the 24-hour heart-rate visualization.



It manages:



\- Hourly BPM indicators

\- BPM color coding

\- Daily statistics

\- Hourly detail panels

\- Exercise, running and walking information

\- Sending selected statistics to the AR view



\### `ARHeartDisplayManager.cs`



Controls the biomedical AR interface.



It manages:



\- AR model/UI visibility

\- Selected date visualization

\- BPM statistics in AR

\- BPM color coding

\- Navigation back to the detailed dashboard



\### `HeartRateAverage.cs`



Data model representing hourly heart-rate information.



\### `ActivityAverage.cs`



Data model representing hourly physical activity information.



\---



\## 📁 Project Structure



```text

Biomedical-AR-Application/

│

├── Assets/

│   ├── Model/

│   ├── Materials/

│   ├── Resources/

│   ├── Scenes/

│   ├── StreamingAssets/

│   ├── Textures/

│   └── \*.cs

│

├── DataProcessing/

│   └── \*.py

│

├── Packages/

├── ProjectSettings/

├── QCAR/

├── UIElementsSchema/

│

├── .gitignore

└── README.md

```



\---



\## ⚙️ Requirements



To run the project you need:



\- Unity

\- Android Build Support

\- Vuforia Engine

\- A compatible Android device for AR testing



The project was developed using \*\*Vuforia Engine 11.2.4\*\*.



\---



\## 📦 Vuforia Setup



The Vuforia `.tgz` package is \*\*not included in this repository\*\* because the local package is larger than GitHub's standard file-size limit.



The Unity package configuration references:



```text

com.ptc.vuforia.engine-11.2.4.tgz

```



Before opening/building the complete project, obtain the corresponding Vuforia Engine package and make it available to Unity according to the package configuration.



Alternatively, Vuforia can be installed through Unity Package Manager following the official Vuforia installation procedure.



\---



\## 🚀 Getting Started



1\. Clone the repository:



```bash

git clone https://github.com/giorghen99/Biomedical-AR-Application.git

```



2\. Open the project with Unity.



3\. Install/configure \*\*Vuforia Engine 11.2.4\*\*.



4\. Verify the Vuforia configuration and image target.



5\. Open the main scene:



```text

Assets/Scenes/SampleScene.unity

```



6\. Configure Android as the build target if required.



7\. Build and run the application on a compatible Android device.



> Depending on the local Vuforia configuration, additional setup such as the appropriate Vuforia credentials/configuration may be required.



\---



\## 🖼️ Screenshots



Screenshots and application previews will be added here.



<!--

Example:



\### AR View

!\[AR View](docs/images/ar-view.png)



\### Heart Rate Dashboard

!\[Dashboard](docs/images/dashboard.png)



\### Hourly Details

!\[Hourly Details](docs/images/hourly-details.png)

\-->



\---



\## ℹ️ Project Context



This project was developed as a \*\*university internship project\*\* with the goal of exploring the integration of:



\- Augmented Reality

\- Real-time 3D visualization

\- Biomedical data visualization

\- Data preprocessing

\- Mobile application development



The datasets included in this project are synthetic and were created exclusively for demonstration and application-development purposes. They do not represent real patients or individuals, although the values were designed to be internally consistent and realistic for the application's use case.



This application is a prototype and is \*\*not intended for medical diagnosis or clinical use\*\*.



\---



\# 🇮🇹 Italiano



\## 📖 Panoramica



\*\*Biomedical AR Application\*\* è un progetto di Realtà Aumentata sviluppato con \*\*Unity, C#, Vuforia e Python\*\* durante un tirocinio universitario.



L'applicazione combina un \*\*modello 3D interattivo di cuore e polmoni\*\* con la visualizzazione di dati biomedicali e relativi all'attività fisica. Attraverso un target AR, l'utente può accedere alla visualizzazione tridimensionale e consultare informazioni sulla frequenza cardiaca associate a diverse date e ore della giornata.



Una dashboard 2D dedicata permette di selezionare una data, analizzare i dati cardiaci delle singole ore, visualizzare statistiche giornaliere e consultare informazioni relative a esercizio, corsa e camminata.



Gli script Python costituiscono uno strato di preprocessing utilizzato per filtrare e aggregare i dataset XML prima del loro caricamento all'interno dell'applicazione Unity.



\---



\## ✨ Funzionalità principali



\- 📱 Visualizzazione in \*\*Realtà Aumentata\*\* tramite Vuforia

\- 🫀 Modello 3D interattivo di \*\*cuore e polmoni\*\*

\- ❤️ Visualizzazione della frequenza cardiaca (BPM)

\- 🕐 Dashboard cardiaca sulle \*\*24 ore\*\*

\- 📅 Selezione della data tramite anno, mese e giorno

\- 📊 Visualizzazione di BPM minimo, massimo e medio giornaliero

\- 🔎 Dettagli relativi alle singole ore

\- 🏃 Informazioni su esercizio e corsa

\- 🚶 Informazioni sulla camminata

\- 🎨 Indicatori BPM basati su colori

\- 🔄 Passaggio tra dashboard 2D e visualizzazione AR

\- 🐍 Preprocessing dei dati XML tramite Python

\- 📱 Implementazione orientata ad Android



\---



\## 🎨 Codifica dei BPM tramite colori



L'applicazione utilizza indicatori visivi per rendere immediatamente leggibili i dati relativi alla frequenza cardiaca.



| Colore | Significato |

|---|---|

| 🟡 Giallo | BPM inferiori a 60 |

| 🟢 Verde | BPM compresi tra 60 e 100 |

| 🔴 Rosso | BPM superiori a 100 |

| ⚫ Grigio | Nessun dato disponibile |



Queste soglie vengono utilizzate come regole di visualizzazione all'interno dell'applicazione e non devono essere interpretate come diagnosi mediche.



\---



\## 📊 Dashboard della frequenza cardiaca



La dashboard principale viene generata dinamicamente utilizzando \*\*Unity UI Toolkit\*\*.



L'utente può selezionare:



\- Anno

\- Mese

\- Giorno



Dopo aver selezionato una data valida, l'applicazione filtra i record disponibili e genera una visualizzazione composta da \*\*24 indicatori\*\*, uno per ogni ora della giornata.



Ogni indicatore cambia colore in funzione del BPM medio registrato per quella determinata ora.



Selezionando un'ora viene aperto un pannello di dettaglio che può mostrare:



\- BPM medio

\- BPM minimo

\- BPM massimo

\- Tempo di esercizio

\- Distanza percorsa correndo

\- Distanza percorsa camminando



Le informazioni relative alla frequenza cardiaca selezionata possono inoltre essere visualizzate direttamente nella modalità AR.



\---



\## 🥽 Realtà Aumentata



Le funzionalità AR sono implementate tramite \*\*Vuforia Engine\*\*.



Quando l'image target configurato viene riconosciuto, l'applicazione visualizza il modello biomedicale 3D e permette di accedere all'interfaccia AR.



L'applicazione utilizza gli eventi di tracking di Vuforia per gestire dinamicamente la visibilità dell'interfaccia AR.



L'utente può passare tra:



\*\*Vista AR → Dashboard dettagliata → Statistiche AR\*\*



Le statistiche cardiache relative alla data o all'ora selezionata possono quindi essere associate alla visualizzazione tridimensionale aumentata.



\---



\## 🐍 Elaborazione dei dati



La directory `DataProcessing` contiene una serie di script Python utilizzati per preparare i dati XML prima del loro utilizzo in Unity.



La pipeline elabora informazioni relative a:



\- Frequenza cardiaca

\- Tempo di esercizio

\- Corsa

\- Camminata



I record vengono raggruppati per \*\*data e ora\*\*, generando dataset XML più piccoli e adatti all'utilizzo nell'applicazione Unity.



\### Pipeline di elaborazione



```text

Dati XML sorgente

&#x20;     │

&#x20;     ▼

Script Python di filtraggio

&#x20;     │

&#x20;     ├── Frequenza cardiaca

&#x20;     ├── Esercizio

&#x20;     ├── Corsa

&#x20;     └── Camminata

&#x20;     │

&#x20;     ▼

Aggregazione oraria

&#x20;     │

&#x20;     ▼

File XML elaborati

&#x20;     │

&#x20;     ▼

Unity StreamingAssets

&#x20;     │

&#x20;     ▼

DynamicUIManager

&#x20;     │

&#x20;     ▼

Dashboard 2D + Visualizzazione AR

```



I file XML utilizzati dall'applicazione vengono caricati a runtime dalla directory `StreamingAssets` di Unity.



Per garantire compatibilità tra le piattaforme, il progetto gestisce sia l'accesso diretto ai file sia il caricamento tramite `UnityWebRequest`.



\---



\## 🛠️ Tecnologie utilizzate



| Tecnologia | Utilizzo |

|---|---|

| Unity | Sviluppo dell'applicazione e grafica 3D real-time |

| C# | Logica applicativa e gestione dell'interfaccia |

| Vuforia Engine | Realtà Aumentata e tracking dell'image target |

| Unity UI Toolkit | Interfaccia utente dinamica |

| TextMeshPro | Rendering dei testi nella visualizzazione AR |

| Python | Preprocessing dei dati biomedicali |

| XML | Memorizzazione dei dati elaborati |

| Android | Piattaforma mobile di destinazione |



\---



\## 🧩 Principali componenti C#



\### `DynamicUIManager.cs`



Coordina l'interfaccia principale e il flusso dei dati.



Si occupa di:



\- Caricamento dei dataset XML

\- Filtraggio per data

\- Calcolo delle statistiche BPM giornaliere

\- Generazione dinamica della UI

\- Comunicazione con il grafico della frequenza cardiaca

\- Gestione degli eventi del target Vuforia

\- Passaggio tra dashboard e visualizzazione AR



\### `HeartRateChart.cs`



Implementa la visualizzazione della frequenza cardiaca sulle 24 ore.



Gestisce:



\- Indicatori BPM orari

\- Codifica cromatica dei BPM

\- Statistiche giornaliere

\- Pannelli di dettaglio delle singole ore

\- Informazioni su esercizio, corsa e camminata

\- Invio delle statistiche selezionate alla vista AR



\### `ARHeartDisplayManager.cs`



Gestisce l'interfaccia biomedicale in Realtà Aumentata.



Si occupa di:



\- Visibilità del modello e dell'interfaccia AR

\- Visualizzazione della data selezionata

\- Statistiche BPM in AR

\- Codifica cromatica dei BPM

\- Ritorno alla dashboard dettagliata



\### `HeartRateAverage.cs`



Modello dati utilizzato per rappresentare le informazioni cardiache orarie.



\### `ActivityAverage.cs`



Modello dati utilizzato per rappresentare le informazioni orarie relative all'attività fisica.



\---



\## 📁 Struttura del progetto



```text

Biomedical-AR-Application/

│

├── Assets/

│   ├── Model/

│   ├── Materials/

│   ├── Resources/

│   ├── Scenes/

│   ├── StreamingAssets/

│   ├── Textures/

│   └── \*.cs

│

├── DataProcessing/

│   └── \*.py

│

├── Packages/

├── ProjectSettings/

├── QCAR/

├── UIElementsSchema/

│

├── .gitignore

└── README.md

```



\---



\## ⚙️ Requisiti



Per eseguire il progetto sono necessari:



\- Unity

\- Android Build Support

\- Vuforia Engine

\- Un dispositivo Android compatibile per i test AR



Il progetto è stato sviluppato utilizzando \*\*Vuforia Engine 11.2.4\*\*.



\---



\## 📦 Configurazione di Vuforia



Il pacchetto `.tgz` di Vuforia \*\*non è incluso nel repository\*\*, poiché il pacchetto locale supera il normale limite di dimensione dei singoli file di GitHub.



La configurazione dei package Unity fa riferimento a:



```text

com.ptc.vuforia.engine-11.2.4.tgz

```



Prima di aprire/compilare completamente il progetto è quindi necessario ottenere il relativo pacchetto Vuforia Engine e renderlo disponibile a Unity in base alla configurazione del progetto.



In alternativa, Vuforia può essere installato tramite Unity Package Manager seguendo la procedura ufficiale di installazione.



\---



\## 🚀 Avvio del progetto



1\. Clonare il repository:



```bash

git clone https://github.com/giorghen99/Biomedical-AR-Application.git

```



2\. Aprire il progetto con Unity.



3\. Installare/configurare \*\*Vuforia Engine 11.2.4\*\*.



4\. Verificare la configurazione di Vuforia e dell'image target.



5\. Aprire la scena principale:



```text

Assets/Scenes/SampleScene.unity

```



6\. Se necessario, configurare Android come piattaforma di build.



7\. Compilare ed eseguire l'applicazione su un dispositivo Android compatibile.



> In base alla configurazione locale di Vuforia potrebbero essere necessari ulteriori passaggi, come la configurazione delle credenziali/impostazioni Vuforia appropriate.



\---



\## 🖼️ Screenshot



Gli screenshot e le anteprime dell'applicazione verranno aggiunti in questa sezione.



<!--

Esempio:



\### Vista AR

!\[Vista AR](docs/images/ar-view.png)



\### Dashboard

!\[Dashboard](docs/images/dashboard.png)



\### Dettaglio orario

!\[Dettaglio orario](docs/images/hourly-details.png)

\-->



\---



\## ℹ️ Contesto del progetto



Il progetto è stato sviluppato nell'ambito di un \*\*tirocinio universitario\*\*, con l'obiettivo di sperimentare l'integrazione tra:



\- Realtà Aumentata

\- Visualizzazione 3D real-time

\- Visualizzazione di dati biomedicali

\- Elaborazione dei dati

\- Sviluppo di applicazioni mobile



I dataset inclusi nel progetto sono sintetici e sono stati creati esclusivamente a scopo dimostrativo e per lo sviluppo dell'applicazione. Non rappresentano pazienti o persone reali, sebbene i valori siano stati costruiti in modo da risultare coerenti e realistici per il caso d'uso dell'applicazione.



L'applicazione è un prototipo e \*\*non è destinata alla diagnosi medica o all'utilizzo clinico\*\*.



\---



\## 👤 Author / Autore



\*\*Antonio Giorgio\*\*



Computer Science graduate interested in real-time development, 3D graphics, Unity and software development.



\---



\## 📄 License



This repository currently does not specify an open-source license. All rights are reserved unless otherwise stated.

