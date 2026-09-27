using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

public class HeartRateChart : VisualElement
{
    // Riferimento al DynamicUIManager per callback
    private DynamicUIManager _dynamicUIManager;

    // Data immagazzinata internamente
    private List<HeartRateAverage> _hourlyHeartRates = new List<HeartRateAverage>();
    private List<ActivityAverage> _hourlyExerciseTimes = new List<ActivityAverage>();
    private List<ActivityAverage> _hourlyRunningDistances = new List<ActivityAverage>();
    private List<ActivityAverage> _hourlyWalkingDistances = new List<ActivityAverage>();

    // Statistiche giornaliere (Min, Max, Avg) che arrivano da DynamicUIManager
    private float _dailyMinBpm;
    private float _dailyMaxBpm;
    private float _dailyAvgBpm;

    // Nuovo campo per memorizzare i dati dell'ora cliccata per l'invio ad AR
    private HeartRateAverage _currentHourlyDataForAR;

    // Colori della UI
    private static Color32 CHART_BACKGROUND_COLOR = new Color32(25, 25, 25, 255);
    private static Color32 GRID_LINE_COLOR = new Color32(50, 50, 50, 255);
    private static Color32 BPM_NORMAL_COLOR = new Color32(0, 200, 0, 255);        // Verde
    private static Color32 BPM_LOW_COLOR = new Color32(255, 255, 0, 255);      // Giallo
    private static Color32 BPM_HIGH_COLOR = new Color32(255, 0, 0, 255);        // Rosso
    private static Color32 BPM_NO_DATA_COLOR = new Color32(100, 100, 100, 255); // Grigio scuro
    private static Color32 TEXT_COLOR = new Color32(240, 240, 240, 255);
    private static Color32 POPUP_BG_COLOR = new Color32(40, 40, 40, 240);
    private static Color32 BUTTON_COLOR = new Color32(0, 150, 200, 255); // Blu per i bottoni
    private static Color32 BUTTON_HOVER_COLOR = new Color32(0, 180, 240, 255);
    private static Color32 WARNING_ORANGE = new Color32(255, 165, 0, 255); // Aggiunto per il bottone disabilitato

    // Soglie BPM (OK qui)
    private const float BPM_THRESHOLD_LOW = 60f;
    private const float BPM_THRESHOLD_HIGH = 100f;

    // Elementi UI interni (OK qui)
    private VisualElement _chartGridContainer;
    private Label _dailyStatsLabel;
    private Button _showARStatsButton;
    private VisualElement _activityDetailPage;
    private Label _detailHourLabel;
    private Label _detailBpmMedioLabel;
    private Label _detailBpmMinLabel;
    private Label _detailBpmMaxLabel;
    private Button _showHourlyARStatsButton;

    // Contenitori e Label per le icone e i valori delle attività nel popup (OK qui)
    private VisualElement _exerciseContainer;
    private VisualElement _runningContainer;
    private VisualElement _walkingContainer;
    private Label _exerciseValueLabel;
    private Label _runningValueLabel;
    private Label _walkingValueLabel;

    // Riferimenti alle Sprite delle icone delle attività (OK qui)
    private Sprite _exerciseIconSprite;
    private Sprite _runningIconSprite;
    private Sprite _walkingIconSprite;


    public HeartRateChart()
    {
        // Carica le icone all'inizializzazione del grafico (OK qui)
        LoadActivityIcons();

        // Styling del container principale del chart (OK qui)
        style.flexGrow = 1;
        style.backgroundColor = new StyleColor(CHART_BACKGROUND_COLOR);
        style.paddingTop = 15;
        style.paddingBottom = 15;
        style.paddingLeft = 10;
        style.paddingRight = 10;
        style.borderTopLeftRadius = new StyleLength(new Length(12, LengthUnit.Pixel));
        style.borderTopRightRadius = new StyleLength(new Length(12, LengthUnit.Pixel));
        style.borderBottomLeftRadius = new StyleLength(new Length(12, LengthUnit.Pixel));
        style.borderBottomRightRadius = new StyleLength(new Length(12, LengthUnit.Pixel));

        // Contenitore per la griglia dei cerchi e le etichette orarie (OK qui)
        _chartGridContainer = new VisualElement();
        _chartGridContainer.style.flexDirection = FlexDirection.Row;
        _chartGridContainer.style.flexWrap = Wrap.Wrap;
        _chartGridContainer.style.justifyContent = Justify.SpaceAround; // O Justify.Center o Justify.FlexStart per allineamento
        _chartGridContainer.style.alignItems = Align.Center;
        _chartGridContainer.style.flexGrow = 1;
        Add(_chartGridContainer);

        // Crea i 24 cerchi e le etichette orarie
        // QUESTA È LA FUNZIONE CHE MODIFICHIAMO
        CreateHourlyCircles();

        // --- Bottone e Label per statistiche giornaliere ---
        _dailyStatsLabel = new Label("BPM Giornaliero: Min -- | Max -- | Medio --");
        _dailyStatsLabel.style.fontSize = 22;
        _dailyStatsLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        _dailyStatsLabel.style.color = new StyleColor(TEXT_COLOR);
        _dailyStatsLabel.style.marginTop = 20;
        _dailyStatsLabel.style.marginBottom = 10;
        Add(_dailyStatsLabel);

        _showARStatsButton = new Button();
        _showARStatsButton.text = "Mostra Statistiche AR";
        _showARStatsButton.style.fontSize = 24;
        _showARStatsButton.style.height = 50;
        _showARStatsButton.style.backgroundColor = new StyleColor(BUTTON_COLOR);
        _showARStatsButton.style.color = new StyleColor(TEXT_COLOR);
        _showARStatsButton.style.unityFontStyleAndWeight = FontStyle.Bold;
        _showARStatsButton.style.borderTopLeftRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        _showARStatsButton.style.borderTopRightRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        _showARStatsButton.style.borderBottomLeftRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        _showARStatsButton.style.borderBottomRightRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        _showARStatsButton.style.marginTop = 10;
        _showARStatsButton.style.display = DisplayStyle.None; // Inizialmente nascosto
        Add(_showARStatsButton);

        _showARStatsButton.RegisterCallback<ClickEvent>(evt =>
        {
            // Quando il bottone viene cliccato, chiama il metodo nel DynamicUIManager
            // e gli passa le statistiche giornaliere attuali.
            if (_dynamicUIManager != null)
            {
                // Qui passiamo i _dailyMinBpm, _dailyMaxBpm, _dailyAvgBpm memorizzati
                _dynamicUIManager.OnDailyStatsSelected(_dailyMinBpm, _dailyMaxBpm, _dailyAvgBpm);
                HideDailyStatsPopup(); // Nascondi la UI del grafico principale
            }
            else
            {
                Debug.LogWarning("HeartRateChart: DynamicUIManager non impostato. Impossibile passare dati ad AR.");
            }
        });

        // --- Crea il pannello dei dettagli attività oraria (inizialmente nascosto) ---
        CreateActivityDetailPage();
        Add(_activityDetailPage); // Aggiunge al root del HeartRateChart (e sarà l'ultimo elemento per essere in cima)

        // Inizialmente nascondi i pannelli
        HideDailyStatsPopup(); // Nasconde la label e il bottone AR giornaliero
        HideActivityDetailPage(); // Nasconde il popup dei dettagli orari
    }

    // Carica le sprite delle icone dalla cartella Resources
    private void LoadActivityIcons()
    {
        _exerciseIconSprite = Resources.Load<Sprite>("Icons/exercise_icon");
        _runningIconSprite = Resources.Load<Sprite>("Icons/running_icon");
        _walkingIconSprite = Resources.Load<Sprite>("Icons/walking_icon");

        if (_exerciseIconSprite == null) Debug.LogWarning("HeartRateChart: Impossibile caricare exercise_icon dalla cartella Assets/Resources/Icons.");
        if (_runningIconSprite == null) Debug.LogWarning("HeartRateChart: Impossibile caricare running_icon dalla cartella Assets/Resources/Icons.");
        if (_walkingIconSprite == null) Debug.LogWarning("HeartRateChart: Impossibile caricare walking_icon dalla cartella Assets/Resources/Icons.");
    }

    /// <summary>
    /// Setter per il riferimento a DynamicUIManager. Chiamato da DynamicUIManager.Start().
    /// </summary>
    public void SetDynamicUIManager(DynamicUIManager manager)
    {
        _dynamicUIManager = manager;
        Debug.Log("HeartRateChart: Riferimento a DynamicUIManager impostato.");
    }

    /// <summary>
    /// Crea i 24 cerchi che rappresentano le ore, con etichette.
    /// </summary>
    private void CreateHourlyCircles()
    {
        int circlesPerRow = 4; // Imposta 4 cerchi per riga
        float circleDesiredSize = 60; // Dimensione desiderata del cerchio in pixel (Larghezza e Altezza)
        float horizontalMargin = 8; // Margine orizzontale tra i cerchi e dai bordi del container
        float verticalMargin = 10; // Margine verticale tra le righe di cerchi

        // Calcoliamo la larghezza percentuale di ogni circleContainer.
        float percentageWidthPerColumn = 100f / circlesPerRow;

        // Aggiungi o verifica questa impostazione sul _chartGridContainer
        // per assicurare che non allunghi i suoi figli in verticale.
        // _chartGridContainer.style.alignItems = Align.FlexStart; // Già impostato su Align.Center, che è ok per centrare i contenuti orizzontalmente.

        for (int i = 0; i < 24; i++)
        {
            VisualElement circleContainer = new VisualElement();
            circleContainer.name = $"hourContainer-{i}";

            // Larghezza calcolata per assicurare 4 per riga
            circleContainer.style.width = new Length(percentageWidthPerColumn, LengthUnit.Percent);
            // Aggiungiamo padding laterale al container per creare spazio tra i cerchi
            circleContainer.style.paddingLeft = horizontalMargin / 2;
            circleContainer.style.paddingRight = horizontalMargin / 2;

            // *** MODIFICA QUI: Rimuovi l'altezza fissa e rendila basata sul contenuto ***
            // o imposta un aspect ratio se UI Toolkit lo supporta direttamente (non sempre ideale)
            // Per ora, non impostiamo un'altezza fissa sul circleContainer
            // circleContainer.style.height = new Length(circleDesiredSize + 20, LengthUnit.Pixel); 

            circleContainer.style.marginTop = verticalMargin;
            circleContainer.style.marginBottom = verticalMargin;
            circleContainer.style.flexDirection = FlexDirection.Column;
            circleContainer.style.alignItems = Align.Center; // Centra il cerchio e la label
            circleContainer.style.justifyContent = Justify.Center; // Centra anche verticalmente i figli nel container
            _chartGridContainer.Add(circleContainer);

            VisualElement circle = new VisualElement();
            circle.name = $"hourCircle-{i}";

            // Imposta la dimensione desiderata del cerchio
            circle.style.width = circleDesiredSize;
            circle.style.height = circleDesiredSize;

            // Il border-radius deve essere la metà della dimensione per un cerchio perfetto
            circle.style.borderTopLeftRadius = new StyleLength(new Length(circleDesiredSize / 2, LengthUnit.Pixel));
            circle.style.borderTopRightRadius = new StyleLength(new Length(circleDesiredSize / 2, LengthUnit.Pixel));
            circle.style.borderBottomLeftRadius = new StyleLength(new Length(circleDesiredSize / 2, LengthUnit.Pixel));
            circle.style.borderBottomRightRadius = new StyleLength(new Length(circleDesiredSize / 2, LengthUnit.Pixel));

            circle.style.backgroundColor = new StyleColor(BPM_NO_DATA_COLOR); // Colore di default "nessun dato"
            circle.style.marginBottom = 5; // Spazio tra cerchio e label
            circleContainer.Add(circle);

            Label hourLabel = new Label(i.ToString("00")); // Formato "00", "01", ..., "23"
            hourLabel.name = $"hourLabel-{i}";
            hourLabel.style.fontSize = 16;
            hourLabel.style.color = new StyleColor(TEXT_COLOR);
            circleContainer.Add(hourLabel);

            // Aggiungi un callback di click al cerchio
            int hour = i; // Cattura la variabile per la closure
            circle.RegisterCallback<ClickEvent>(evt => OnHourlyCircleClicked(hour));
        }
    }

    /// <summary>
    /// Crea il VisualElement per il pannello dei dettagli dell'attività oraria.
    /// </summary>
    private void CreateActivityDetailPage()
    {
        _activityDetailPage = new VisualElement();
        _activityDetailPage.style.position = Position.Absolute;
        _activityDetailPage.style.top = 0;
        _activityDetailPage.style.left = 0;
        _activityDetailPage.style.right = 0;
        _activityDetailPage.style.bottom = 0;
        _activityDetailPage.style.backgroundColor = new StyleColor(new Color32(18, 18, 18, 220)); // Sfondo semitrasparente
        _activityDetailPage.style.justifyContent = Justify.Center;
        _activityDetailPage.style.alignItems = Align.Center;
        _activityDetailPage.style.flexDirection = FlexDirection.Column;
        _activityDetailPage.style.display = DisplayStyle.None; // Nascosto di default

        VisualElement contentContainer = new VisualElement();
        contentContainer.style.backgroundColor = new StyleColor(new Color32(35, 35, 50, 240));
        contentContainer.style.borderTopLeftRadius = new StyleLength(new Length(16, LengthUnit.Pixel));
        contentContainer.style.borderTopRightRadius = new StyleLength(new Length(16, LengthUnit.Pixel));
        contentContainer.style.borderBottomLeftRadius = new StyleLength(new Length(16, LengthUnit.Pixel));
        contentContainer.style.borderBottomRightRadius = new StyleLength(new Length(16, LengthUnit.Pixel));
        contentContainer.style.paddingLeft = 30;
        contentContainer.style.paddingRight = 30;
        contentContainer.style.paddingTop = 30;
        contentContainer.style.paddingBottom = 30;
        contentContainer.style.alignItems = Align.Center;
        _activityDetailPage.Add(contentContainer);

        Label title = new Label("Dettagli Attività Oraria");
        title.style.fontSize = 32;
        title.style.color = new StyleColor(TEXT_COLOR); // Usare StyleColor
        title.style.marginBottom = 20;
        contentContainer.Add(title);

        _detailHourLabel = new Label("Dettagli per l'ora: --");
        _detailHourLabel.style.fontSize = 24;
        _detailHourLabel.style.color = new StyleColor(TEXT_COLOR); // Usare StyleColor
        _detailHourLabel.style.marginBottom = 15;
        contentContainer.Add(_detailHourLabel);

        _detailBpmMedioLabel = new Label("BPM Medio: --");
        _detailBpmMedioLabel.style.fontSize = 20;
        _detailBpmMedioLabel.style.color = new StyleColor(TEXT_COLOR); // Usare StyleColor
        contentContainer.Add(_detailBpmMedioLabel);

        // Aggiungi queste righe per dare un nome alle label, utile per nasconderle/mostrarle
        _detailBpmMinLabel = new Label("BPM Min: --");
        _detailBpmMinLabel.name = "DetailBpmMinLabel"; // Aggiunto nome
        _detailBpmMinLabel.style.fontSize = 20;
        _detailBpmMinLabel.style.color = new StyleColor(TEXT_COLOR); // Usare StyleColor
        contentContainer.Add(_detailBpmMinLabel);

        _detailBpmMaxLabel = new Label("BPM Max: --");
        _detailBpmMaxLabel.name = "DetailBpmMaxLabel"; // Aggiunto nome
        _detailBpmMaxLabel.style.fontSize = 20;
        _detailBpmMaxLabel.style.color = new StyleColor(TEXT_COLOR); // Usare StyleColor
        contentContainer.Add(_detailBpmMaxLabel);

        VisualElement separator = new VisualElement();
        separator.style.height = 1;
        separator.style.width = new Length(80, LengthUnit.Percent);
        separator.style.backgroundColor = new StyleColor(new Color32(100, 100, 100, 255));
        separator.style.marginTop = 20;
        separator.style.marginBottom = 20;
        contentContainer.Add(separator);

        // Aggiungi i contenitori per le icone e i testi delle attività
        _exerciseContainer = CreateActivityRow(_exerciseIconSprite, "Nessun dato Esercizio");
        _runningContainer = CreateActivityRow(_runningIconSprite, "Distanza Corsa: -- km");
        _walkingContainer = CreateActivityRow(_walkingIconSprite, "Distanza Camminata: -- km");

        // Associa le label interne ai riferimenti per aggiornarle
        _exerciseValueLabel = _exerciseContainer.Q<Label>("ActivityValueLabel");
        _runningValueLabel = _runningContainer.Q<Label>("ActivityValueLabel");
        _walkingValueLabel = _walkingContainer.Q<Label>("ActivityValueLabel");

        contentContainer.Add(_exerciseContainer);
        contentContainer.Add(_runningContainer);
        contentContainer.Add(_walkingContainer);

        // Bottone "Mostra Statistiche Orarie AR"
        _showHourlyARStatsButton = new Button();
        _showHourlyARStatsButton.text = "Mostra Statistiche Orarie AR";
        _showHourlyARStatsButton.name = "showHourlyARStatsButton"; // Nome per query
        _showHourlyARStatsButton.style.fontSize = 24;
        _showHourlyARStatsButton.style.height = 50;
        _showHourlyARStatsButton.style.backgroundColor = new StyleColor(BUTTON_COLOR);
        _showHourlyARStatsButton.style.color = new StyleColor(TEXT_COLOR);
        _showHourlyARStatsButton.style.unityFontStyleAndWeight = FontStyle.Bold;
        _showHourlyARStatsButton.style.borderTopLeftRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        _showHourlyARStatsButton.style.borderTopRightRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        _showHourlyARStatsButton.style.borderBottomLeftRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        _showHourlyARStatsButton.style.borderBottomRightRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        _showHourlyARStatsButton.style.marginTop = 20; // Margine per separarlo
        contentContainer.Add(_showHourlyARStatsButton);

        // Registra il callback per il nuovo bottone
        _showHourlyARStatsButton.RegisterCallback<ClickEvent>(evt =>
        {
            if (_dynamicUIManager == null)
            {
                Debug.LogWarning("HeartRateChart: DynamicUIManager non impostato per le statistiche AR orarie.");
                return; // Esce per evitare NullReferenceException
            }

            // CONTROLLO FONDAMENTALE: Verifica se _currentHourlyDataForAR ha dati validi
            if (_currentHourlyDataForAR != null && _currentHourlyDataForAR.average > 0) // Controlla anche che average sia > 0
            {
                // Ora passiamo i veri MinBpm e MaxBpm orari
                _dynamicUIManager.OnDailyStatsSelected(
                    _currentHourlyDataForAR.MinBpm,    // Usa il vero min orario
                    _currentHourlyDataForAR.MaxBpm,    // Usa il vero max orario
                    _currentHourlyDataForAR.average    // Usa la media oraria
                );
                HideActivityDetailPage(); // Nascondi il popup dei dettagli orari dopo l'invio ad AR
            }
            else
            {
                // Questo è il warning che hai visto, ora lo gestiamo meglio
                Debug.LogWarning("HeartRateChart: Nessun dato di battito cardiaco disponibile per l'ora selezionata per AR.");
                // Puoi anche mostrare un messaggio all'utente o disabilitare il bottone se non ci sono dati validi
                // In questo caso, potresti anche voler chiamare _dynamicUIManager.OnDailyStatsSelected(0,0,0)
                // per assicurarti che l'AR sia nascosto se non ci sono dati da mostrare.
                _dynamicUIManager.OnDailyStatsSelected(0, 0, 0); // Nasconde l'AR se non ci sono dati
                // Opzionale: mostrare un Toast o un messaggio temporaneo all'utente nella UI
            }
        });

        // Bottone "Chiudi"
        Button closeDetailButton = new Button(() => HideActivityDetailPage());
        closeDetailButton.text = "Chiudi";
        closeDetailButton.style.fontSize = 24;
        closeDetailButton.style.width = 150;
        closeDetailButton.style.height = 50;
        closeDetailButton.style.backgroundColor = new StyleColor(BUTTON_COLOR);
        closeDetailButton.style.color = new StyleColor(TEXT_COLOR);
        closeDetailButton.style.borderTopLeftRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        closeDetailButton.style.borderTopRightRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        closeDetailButton.style.borderBottomLeftRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        closeDetailButton.style.borderBottomRightRadius = new StyleLength(new Length(8, LengthUnit.Pixel));
        closeDetailButton.style.marginTop = 30;
        contentContainer.Add(closeDetailButton);
    }

    // Metodo helper per creare una riga di attività con icona e label
    private VisualElement CreateActivityRow(Sprite iconSprite, string initialText)
    {
        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.marginBottom = 10;

        Image icon = new Image();
        icon.sprite = iconSprite;
        icon.style.width = 40;
        icon.style.height = 40;
        icon.style.marginRight = 10;
        // Rimosso unityBackgroundScaleMode obsoleto.
        // Se vuoi che l'immagine si ridimensioni per adattarsi all'elemento,
        // dovresti usare style.backgroundSize (se imposti l'immagine come background)
        // o assicurarti che l'Image sia impostata per riempire (ScaleMode.StretchToFill è il default per Image).
        // Per icone, spesso ScaleMode.ScaleToFit è desiderabile, ma UI Toolkit vuole BackgroundSize.Contain
        // se l'immagine è impostata via background-image.
        // Poiché qui usi sprite = iconSprite, gestisce automaticamente.
        // Se volessi replicare ScaleMode.ScaleToFit per un background-image fittizio:
        // icon.style.backgroundSize = BackgroundSize.Contain; // Questo va su StyleBackground
        // Ma per Image.sprite, non è necessario.

        row.Add(icon);

        Label valueLabel = new Label(initialText);
        valueLabel.name = "ActivityValueLabel";
        valueLabel.style.fontSize = 20;
        valueLabel.style.color = new StyleColor(TEXT_COLOR); // Usare StyleColor
        row.Add(valueLabel);

        return row;
    }

    /// <summary>
    /// Imposta i dati per il grafico. Richiamato da DynamicUIManager quando la data cambia.
    /// </summary>
    public void SetData(List<HeartRateAverage> heartRates, List<ActivityAverage> exerciseTimes, List<ActivityAverage> runningDistances, List<ActivityAverage> walkingDistances, float minBpm, float maxBpm, float avgBpm)
    {
        _hourlyHeartRates = heartRates;
        _hourlyExerciseTimes = exerciseTimes;
        _hourlyRunningDistances = runningDistances;
        _hourlyWalkingDistances = walkingDistances;

        _dailyMinBpm = minBpm;
        _dailyMaxBpm = maxBpm;
        _dailyAvgBpm = avgBpm;

        UpdateChartDisplay();
        UpdateDailyStatsDisplay();
        HideActivityDetailPage(); // Assicurati che il dettaglio attività sia nascosto quando i dati cambiano
    }

    /// <summary>
    /// Aggiorna la visualizzazione dei 24 cerchi in base ai dati correnti.
    /// </summary>
    private void UpdateChartDisplay()
    {
        for (int i = 0; i < 24; i++)
        {
            VisualElement circle = _chartGridContainer.Q<VisualElement>($"hourCircle-{i}");
            if (circle == null) continue;

            HeartRateAverage hourlyData = _hourlyHeartRates.FirstOrDefault(h => h.hour == i);

            if (hourlyData != null)
            {
                circle.style.backgroundColor = GetBpmColor(hourlyData.average);
            }
            else
            {
                circle.style.backgroundColor = new StyleColor(BPM_NO_DATA_COLOR); // Nessun dato per quest'ora
            }
        }
    }

    /// <summary>
    /// Aggiorna il testo delle statistiche giornaliere e la visibilità del bottone AR.
    /// </summary>
    public void UpdateDailyStatsDisplay()
    {
        // Controlla se ci sono dati di battito cardiaco per il giorno selezionato
        bool hasDailyHeartRateData = _hourlyHeartRates != null && _hourlyHeartRates.Any(h => h.average > 0);

        if (hasDailyHeartRateData)
        {
            _dailyStatsLabel.text = $"BPM Giornaliero: Min {_dailyMinBpm:F0} | Max {_dailyMaxBpm:F0} | Medio {_dailyAvgBpm:F0}";
            _dailyStatsLabel.style.display = DisplayStyle.Flex;
            _showARStatsButton.text = "Mostra Statistiche AR"; // Ripristina testo bottone
            _showARStatsButton.style.backgroundColor = new StyleColor(BUTTON_COLOR); // Ripristina colore
            _showARStatsButton.SetEnabled(true); // Abilita il bottone
            _showARStatsButton.style.display = DisplayStyle.Flex;
        }
        else
        {
            _dailyStatsLabel.text = "Nessun dato BPM per questa data.";
            _dailyStatsLabel.style.display = DisplayStyle.Flex; // Mostra comunque il messaggio
            _showARStatsButton.text = "Nessun dato BPM per AR"; // Messaggio più chiaro
            _showARStatsButton.style.backgroundColor = new StyleColor(WARNING_ORANGE); // Colore di avviso
            _showARStatsButton.SetEnabled(false); // Disabilita il bottone
            _showARStatsButton.style.display = DisplayStyle.Flex; // Mostra il bottone disabilitato
        }
    }

    /// <summary>
    /// Restituisce un colore basato sul BPM rispetto alle soglie.
    /// </summary>
    private StyleColor GetBpmColor(float bpm)
    {
        if (bpm <= 0) return new StyleColor(BPM_NO_DATA_COLOR);
        if (bpm < BPM_THRESHOLD_LOW) return new StyleColor(BPM_LOW_COLOR);
        if (bpm > BPM_THRESHOLD_HIGH) return new StyleColor(BPM_HIGH_COLOR);
        return new StyleColor(BPM_NORMAL_COLOR);
    }

    /// <summary>
    /// Gestisce il click su un cerchio orario, mostrando i dettagli attività.
    /// </summary>
    private void OnHourlyCircleClicked(int hour)
    {
        Debug.Log($"HeartRateChart: Clicked on hour {hour}");

        // Battito cardiaco
        HeartRateAverage hr = _hourlyHeartRates.FirstOrDefault(h => h.hour == hour);
        _currentHourlyDataForAR = hr; // Memorizza i dati dell'ora cliccata per il bottone AR nel popup

        // Dati attività
        ActivityAverage exercise = _hourlyExerciseTimes.FirstOrDefault(a => a.hour == hour);
        ActivityAverage running = _hourlyRunningDistances.FirstOrDefault(a => a.hour == hour);
        ActivityAverage walking = _hourlyWalkingDistances.FirstOrDefault(a => a.hour == hour);

        ShowActivityDetailPage(hour, hr, exercise, running, walking); // Passa tutti i dati al metodo di visualizzazione
    }

    /// <summary>
    /// Mostra la pagina di dettaglio attività con il contenuto specificato.
    /// </summary>
    public void ShowActivityDetailPage(int hour, HeartRateAverage hr, ActivityAverage exercise, ActivityAverage running, ActivityAverage walking)
    {
        // Aggiorna le label dei dettagli BPM
        _detailHourLabel.text = $"Dettagli per l'ora {hour:00}:";

        if (hr != null && hr.average > 0) // Assicurati che ci siano dati validi
        {
            // Controlla se Min, Max e Medio sono tutti uguali
            if (hr.MinBpm == hr.MaxBpm && hr.MaxBpm == hr.average)
            {
                _detailBpmMedioLabel.text = $"BPM: {hr.average:F0}"; // Mostra solo un valore
                _detailBpmMinLabel.style.display = DisplayStyle.None; // Nascondi Min
                _detailBpmMaxLabel.style.display = DisplayStyle.None; // Nascondi Max
            }
            else
            {
                // Mostra tutti e tre i valori
                _detailBpmMedioLabel.text = $"BPM Medio: {hr.average:F0}";
                _detailBpmMinLabel.text = $"BPM Min: {hr.MinBpm:F0}";
                _detailBpmMaxLabel.text = $"BPM Max: {hr.MaxBpm:F0}";
                _detailBpmMinLabel.style.display = DisplayStyle.Flex; // Mostra Min
                _detailBpmMaxLabel.style.display = DisplayStyle.Flex; // Mostra Max
            }

            // Abilita il bottone AR orario e ripristina il suo stato
            _showHourlyARStatsButton.text = "Mostra Statistiche Orarie AR";
            _showHourlyARStatsButton.style.backgroundColor = new StyleColor(BUTTON_COLOR);
            _showHourlyARStatsButton.SetEnabled(true);
        }
        else
        {
            // Nessun dato BPM
            _detailBpmMedioLabel.text = "BPM: --"; // Mostra un singolo valore per "nessun dato"
            _detailBpmMinLabel.style.display = DisplayStyle.None; // Nascondi Min
            _detailBpmMaxLabel.style.display = DisplayStyle.None; // Nascondi Max

            // Disabilita il bottone AR orario e mostra un messaggio
            _showHourlyARStatsButton.text = "Nessun dato BPM per l'ora selezionata";
            _showHourlyARStatsButton.style.backgroundColor = new StyleColor(WARNING_ORANGE);
            _showHourlyARStatsButton.SetEnabled(false);
        }

        // Aggiorna le label dei valori delle attività (questo rimane invariato e va bene così)
        _exerciseValueLabel.text = (exercise != null && exercise.value > 0) ? $"Tempo di Esercizio: {exercise.value:F0} minuti" : "Nessun dato Esercizio";
        _runningValueLabel.text = (running != null && running.value > 0) ? $"Distanza Corsa: {running.value:F2} km" : "Nessun dato Corsa";
        _walkingValueLabel.text = (walking != null && walking.value > 0) ? $"Distanza Camminata: {walking.value:F2} km" : "Nessun dato Camminata";

        _activityDetailPage.style.display = DisplayStyle.Flex;
        _activityDetailPage.BringToFront();
    }

    /// <summary>
    /// Nasconde la pagina di dettaglio attività.
    /// </summary>
    public void HideActivityDetailPage()
    {
        _activityDetailPage.style.display = DisplayStyle.None;
        // Quando si chiude il popup dei dettagli orari, si può decidere se mostrare di nuovo il popup giornaliero
        // Per ora, lo nascondo sempre per coerenza, poi DynamicUIManager lo mostrerà se necessario.
        // UpdateDailyStatsDisplay(); // Non chiamarlo qui per evitare loop o comportamenti inattesi
    }

    /// <summary>
    /// Nasconde il popup delle statistiche giornaliere (la label e il bottone AR).
    /// </summary>
    public void HideDailyStatsPopup()
    {
        _dailyStatsLabel.style.display = DisplayStyle.None;
        _showARStatsButton.style.display = DisplayStyle.None;
    }
}