using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Xml.Linq;
using System.Collections;
using UnityEngine.Networking;
using Vuforia;

public class DynamicUIManager : MonoBehaviour
{
    public UIDocument uiDocument;

    [Header("AR Display References")]
    [SerializeField] private ARHeartDisplayManager arHeartDisplayManager;
    [SerializeField] private StyleSheet dropdownStyleSheet;

    private List<HeartRateAverage> _allHeartRateAverages = new List<HeartRateAverage>();
    private List<ActivityAverage> _allExerciseAverages = new List<ActivityAverage>();
    private List<ActivityAverage> _allRunningAverages = new List<ActivityAverage>();
    private List<ActivityAverage> _allWalkingAverages = new List<ActivityAverage>();

    private DropdownField _yearDropdown;
    private DropdownField _monthDropdown;
    private DropdownField _dayDropdown;

    private VisualElement _centralContainer; // Il container principale del menu (data, grafico)
    private VisualElement _chartAreaContainer;
    private HeartRateChart _heartRateChart;

    // Inizializza _currentSelectedDate a DateTime.MinValue per indicare nessuna selezione iniziale
    private DateTime _currentSelectedDate = DateTime.MinValue;

    private static Color32 PRIMARY_DARK_BACKGROUND = new Color32(18, 18, 18, 255);
    private static Color32 SECONDARY_CONTAINER_BG = new Color32(30, 30, 30, 240);
    private static Color32 ACCENT_BLUE_BRIGHT = new Color32(0, 192, 255, 255);
    private static Color32 ACCENT_BLUE_DARK = new Color32(0, 120, 180, 255);
    private static Color32 TEXT_COLOR_PRIMARY = new Color32(240, 240, 240, 255);

    private DefaultObserverEventHandler _defaultObserverEventHandler;

    private Coroutine _showUiCoroutine; // Usato per il delay iniziale per la UI AR

    private bool _isTargetCurrentlyTracked = false;
    private bool _hasFullUIMenuBeenShownInitially = false; // Indica se il menu completo è stato mostrato almeno una volta (per il reset iniziale dei dropdown)
    private bool _isFullUIMenuCurrentlyVisible = false; // Indica se il menu completo (quello 2D, con grafici) è attualmente visibile

    // Nuovo flag per la persistenza della UI AR
    private bool _shouldARDisplayPersist = false; // True se la UI AR dovrebbe rimanere visibile indipendentemente dal tracking del target

    void Start()
    {
        if (uiDocument == null)
        {
            GameObject uiDocGO = new GameObject("DynamicUIDocument_Runtime");
            uiDocument = uiDocGO.AddComponent<UIDocument>();
            Debug.LogWarning("DynamicUIManager: UIDocument non assegnato nell'Inspector. Creato uno nuovo a runtime: 'DynamicUIManager_Runtime'.");
        }

        VisualElement root = uiDocument.rootVisualElement;
        root.Clear();
        root.style.backgroundColor = new StyleColor(new Color32(0, 0, 0, 0));

        if (dropdownStyleSheet != null)
        {
            root.styleSheets.Add(dropdownStyleSheet);
        }
        else
        {
            Debug.LogWarning("DynamicUIManager: Style Sheet per i dropdown non assegnato nell'Inspector.");
        }

        root.style.backgroundColor = new StyleColor(new Color32(0, 0, 0, 0));

        StartCoroutine(LoadAllRecordsCoroutine());

        _centralContainer = new VisualElement();
        _centralContainer.style.position = Position.Absolute;
        _centralContainer.style.width = new Length(90, LengthUnit.Percent);
        _centralContainer.style.height = new Length(90, LengthUnit.Percent);
        _centralContainer.style.left = new Length(5, LengthUnit.Percent);
        _centralContainer.style.top = new Length(5, LengthUnit.Percent);

        _centralContainer.style.backgroundColor = new StyleColor(SECONDARY_CONTAINER_BG);
        _centralContainer.style.borderLeftColor = new StyleColor(new Color32(80, 80, 80, 255));
        _centralContainer.style.borderRightColor = new StyleColor(new Color32(80, 80, 80, 255));
        _centralContainer.style.borderTopColor = new StyleColor(new Color32(80, 80, 80, 255));
        _centralContainer.style.borderBottomColor = new StyleColor(new Color32(80, 80, 80, 255));
        _centralContainer.style.borderLeftWidth = 1;
        _centralContainer.style.borderRightWidth = 1;
        _centralContainer.style.borderTopWidth = 1;
        _centralContainer.style.borderBottomWidth = 1;
        _centralContainer.style.borderTopLeftRadius = 16;
        _centralContainer.style.borderTopRightRadius = 16;
        _centralContainer.style.borderBottomLeftRadius = 16;
        _centralContainer.style.borderBottomRightRadius = 16;

        _centralContainer.style.paddingLeft = 20;
        _centralContainer.style.paddingRight = 20;
        _centralContainer.style.paddingTop = 20;
        _centralContainer.style.paddingBottom = 20;

        _centralContainer.style.flexDirection = FlexDirection.Column;
        _centralContainer.style.display = DisplayStyle.None; // Nascosto di default all'avvio
        root.Add(_centralContainer);

        // --- Titolo ---
        Label titleLabel = new Label("MONITORAGGIO BATTITO CARDIACO");
        titleLabel.style.fontSize = 36;
        titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        titleLabel.style.color = new StyleColor(TEXT_COLOR_PRIMARY);
        titleLabel.style.marginBottom = 30;
        _centralContainer.Add(titleLabel);

        // --- Contenitore Selettori Data ---
        VisualElement dateSelectorContainer = new VisualElement();
        dateSelectorContainer.style.flexDirection = FlexDirection.Row;
        dateSelectorContainer.style.justifyContent = Justify.SpaceAround;
        dateSelectorContainer.style.alignItems = Align.Center;
        dateSelectorContainer.style.paddingLeft = 12;
        dateSelectorContainer.style.paddingRight = 12;
        dateSelectorContainer.style.paddingTop = 12;
        dateSelectorContainer.style.paddingBottom = 12;
        dateSelectorContainer.style.height = 75;
        dateSelectorContainer.style.backgroundColor = new StyleColor(new Color32(35, 35, 50, 210));
        dateSelectorContainer.style.borderTopLeftRadius = 12;
        dateSelectorContainer.style.borderTopRightRadius = 12;
        dateSelectorContainer.style.borderBottomLeftRadius = 12;
        dateSelectorContainer.style.borderBottomRightRadius = 12;
        dateSelectorContainer.style.marginBottom = 25;
        _centralContainer.Add(dateSelectorContainer);

        // --- Dropdown Anno ---
        _yearDropdown = new DropdownField("ANNO:");
        _yearDropdown.style.flexGrow = 1;
        _yearDropdown.style.marginRight = 15;
        _yearDropdown.style.fontSize = 20;
        _yearDropdown.style.color = new StyleColor(Color.white);
        dateSelectorContainer.Add(_yearDropdown);

        // --- Dropdown Mese ---
        _monthDropdown = new DropdownField("MESE:");
        _monthDropdown.choices.Add("-");
        for (int i = 1; i <= 12; i++)
        {
            _monthDropdown.choices.Add(i.ToString("00"));
        }
        _monthDropdown.value = "-";
        _monthDropdown.style.flexGrow = 1;
        _monthDropdown.style.marginRight = 15;
        _monthDropdown.style.fontSize = 20;
        _monthDropdown.style.color = new StyleColor(Color.white);
        dateSelectorContainer.Add(_monthDropdown);

        // --- Dropdown Giorno ---
        _dayDropdown = new DropdownField("GIORNO:");
        _dayDropdown.choices.Add("-");
        _dayDropdown.value = "-";
        _dayDropdown.style.flexGrow = 1;
        _dayDropdown.style.fontSize = 20;
        _dayDropdown.style.color = new StyleColor(Color.white);
        dateSelectorContainer.Add(_dayDropdown);

        ChangeDropdownLabelColor(_yearDropdown);
        ChangeDropdownLabelColor(_monthDropdown);
        ChangeDropdownLabelColor(_dayDropdown);

        _yearDropdown.RegisterValueChangedCallback(evt => OnDateSelectionChanged());
        _monthDropdown.RegisterValueChangedCallback(evt => OnDateSelectionChanged());
        _dayDropdown.RegisterValueChangedCallback(evt => OnDateSelectionChanged());

        // --- Contenitore dell'Area Grafici ---
        _chartAreaContainer = new VisualElement();
        _chartAreaContainer.style.flexDirection = FlexDirection.Column;
        _chartAreaContainer.style.flexGrow = 1;
        _chartAreaContainer.style.borderTopLeftRadius = 12;
        _chartAreaContainer.style.borderTopRightRadius = 12;
        _chartAreaContainer.style.borderBottomLeftRadius = 12;
        _chartAreaContainer.style.borderBottomRightRadius = 12;
        _chartAreaContainer.style.backgroundColor = new StyleColor(SECONDARY_CONTAINER_BG);
        _chartAreaContainer.style.paddingLeft = 10;
        _chartAreaContainer.style.paddingRight = 10;
        _chartAreaContainer.style.paddingTop = 10;
        _chartAreaContainer.style.paddingBottom = 10;
        _centralContainer.Add(_chartAreaContainer);

        _heartRateChart = new HeartRateChart();
        _heartRateChart.name = "HeartRateChart";
        _heartRateChart.style.flexGrow = 1;
        _heartRateChart.style.minHeight = 250;
        _chartAreaContainer.Add(_heartRateChart);

        if (arHeartDisplayManager != null)
        {
            arHeartDisplayManager.SetDynamicUIManager(this);
        }
        else
        {
            Debug.LogWarning("DynamicUIManager: ARHeartDisplayManager non assegnato nell'Inspector.");
        }

        _defaultObserverEventHandler = FindAnyObjectByType<DefaultObserverEventHandler>();
        if (_defaultObserverEventHandler != null)
        {
            _defaultObserverEventHandler.OnTargetFound.AddListener(OnTargetFound);
            _defaultObserverEventHandler.OnTargetLost.AddListener(OnTargetLost);
            Debug.Log("DynamicUIManager: Iscritto agli eventi del DefaultObserverEventHandler del target Vuforia.");
        }
        else
        {
            Debug.LogError("DynamicUIManager: Impossibile trovare un DefaultObserverEventHandler nella scena. La UI non apparirà automaticamente in AR.");
        }

        // Chiamata a una nuova coroutine per inizializzare gli elementi UI dopo un frame
        StartCoroutine(InitializeUIElementsCoroutine());

        ToggleFullUIMenuVisibility(false);
    }

    private void ChangeDropdownLabelColor(DropdownField dropdown)
    {
        // Il campo di testo che appare nel dropdown quando un valore è selezionato
        var labelElement = dropdown.Q<TextElement>(className: "unity-base-field__label");
        if (labelElement != null)
        {
            labelElement.style.color = new StyleColor(Color.white);
        }
    }

    // Nuovo metodo per inizializzare gli elementi UI dopo la costruzione iniziale
    private IEnumerator InitializeUIElementsCoroutine()
    {
        yield return null; // Attendi un frame per permettere alla gerarchia di essere processata

        if (_heartRateChart != null)
        {
            _heartRateChart.BringToFront();
            _heartRateChart.SetDynamicUIManager(this);
            _heartRateChart.HideDailyStatsPopup();
            _heartRateChart.HideActivityDetailPage();
            Debug.Log("DynamicUIManager: _heartRateChart inizializzato e popup/dettagli nascosti.");
        }
        else
        {
            Debug.LogError("DynamicUIManager: _heartRateChart è NULL dopo il frame di attesa. Inizializzazione fallita.");
        }
    }


    public void OnDailyStatsSelected(float minBpm, float maxBpm, float avgBpm)
    {
        Debug.Log($"DynamicUIManager.OnDailyStatsSelected: Ricevuti dati per AR - Min:{minBpm:F2}, Max:{maxBpm:F2}, Avg:{avgBpm:F2}");

        // Nasconde la UI del grafico principale
        ToggleFullUIMenuVisibility(false);

        // Mostra le statistiche BPM in AR
        if (arHeartDisplayManager != null)
        {
            bool ciSonoDatiBPMValidi = avgBpm > 0 || minBpm > 0 || maxBpm > 0;

            if (ciSonoDatiBPMValidi)
            {
                arHeartDisplayManager.UpdateARDisplay(minBpm, maxBpm, avgBpm, _currentSelectedDate);
            }
            else
            {
                arHeartDisplayManager.InitializeAndShowARDisplay();
                Debug.Log("DynamicUIManager: Nessun dato BPM valido per la data selezionata. Mostrato solo cuore/polmoni e bottone 'Mostra Dettagli'.");
            }
            _shouldARDisplayPersist = true; // Dopo aver selezionato dati giornalieri (e quindi aver gestito la UI AR), la UI AR dovrebbe persistere
        }
        else
        {
            Debug.LogWarning("DynamicUIManager: Riferimento a ARHeartDisplayManager non assegnato nell'Inspector. Le statistiche AR non saranno visualizzate.");
        }
    }

    private void OnTargetFound()
    {
        Debug.Log("DynamicUIManager: Target Vuforia trovato.");
        _isTargetCurrentlyTracked = true;

        if (_showUiCoroutine != null)
        {
            StopCoroutine(_showUiCoroutine);
        }

        // Se non abbiamo ancora esplicitamente deciso di persistere la UI AR,
        // o se il menu completo è attualmente visibile, mostra la UI AR dopo un delay.
        // Altrimenti, se siamo già in uno stato di persistenza (ad esempio, tornati dal menu completo),
        // la UI AR sarà già visibile o verrà gestita dalla funzione di ritorno.
        if (!_shouldARDisplayPersist && !_isFullUIMenuCurrentlyVisible)
        {
            _showUiCoroutine = StartCoroutine(ShowARDisplayWithDelay(3f));
        }
        else if (_shouldARDisplayPersist && !_isFullUIMenuCurrentlyVisible)
        {
            // Se la UI AR deve persistere e il menu completo non è visibile, assicurati che la UI AR sia mostrata
            if (arHeartDisplayManager != null)
            {
                arHeartDisplayManager.InitializeAndShowARDisplay(); // O UpdateARDisplay con i dati attuali
                Debug.Log("DynamicUIManager: Target trovato e AR Display persistente attivo. Assicurato che la UI AR sia visibile.");
            }
        }
    }

    private void OnTargetLost()
    {
        Debug.Log("DynamicUIManager: Target Vuforia perso.");
        _isTargetCurrentlyTracked = false;

        if (_showUiCoroutine != null)
        {
            StopCoroutine(_showUiCoroutine);
            _showUiCoroutine = null;
        }

        // Modifica qui: la UI AR scompare SOLO se non è impostata per persistere
        // e se il menu completo non è attualmente visibile (per evitare conflitti)
        if (!_shouldARDisplayPersist && !_isFullUIMenuCurrentlyVisible)
        {
            if (arHeartDisplayManager != null)
            {
                arHeartDisplayManager.HideARDisplay();
                Debug.Log("DynamicUIManager: Target perso, UI AR nascosta (non in modalità persistenza).");
            }
        }
        else
        {
            Debug.Log("DynamicUIManager: Target perso, ma UI AR NON nascosta perché è in modalità persistenza o il menu completo è visibile.");
        }
    }

    /// <summary>
    /// Coroutine per mostrare solo il display AR (cuore e bottone menu) dopo un delay.
    /// </summary>
    private IEnumerator ShowARDisplayWithDelay(float delaySeconds)
    {
        Debug.Log($"DynamicUIManager: Attendo {delaySeconds} secondi prima di mostrare il display AR iniziale.");
        yield return new WaitForSeconds(delaySeconds);

        if (_isTargetCurrentlyTracked && !_isFullUIMenuCurrentlyVisible) // Assicurati che non si attivi se il menu completo è già aperto
        {
            Debug.Log("DynamicUIManager: Ritardo terminato, il target è ancora tracciato. Mostro il display AR iniziale.");
            if (arHeartDisplayManager != null)
            {
                arHeartDisplayManager.InitializeAndShowARDisplay();
            }
        }
        else if (!_isTargetCurrentlyTracked) // Se il target si è perso durante il delay
        {
            Debug.Log("DynamicUIManager: Il target è stato perso durante il ritardo. Non mostro il display AR iniziale.");
            if (arHeartDisplayManager != null)
            {
                arHeartDisplayManager.HideARDisplay();
            }
        }
        _showUiCoroutine = null;
    }


    private IEnumerator LoadAllRecordsCoroutine()
    {
        Debug.Log("DynamicUIManager: Avvio caricamento di tutti i record sul telefono.");

        yield return StartCoroutine(LoadXmlAveragesCoroutine<HeartRateAverage>("heart_rate_media.xml", "HeartRateAverage", records => _allHeartRateAverages = records));
        Debug.Log($"DynamicUIManager: Caricati {_allHeartRateAverages.Count} record di battito cardiaco.");

        yield return StartCoroutine(LoadXmlAveragesCoroutine<ActivityAverage>("exercise_medie_per_ora_giorno.xml", "Average", records => _allExerciseAverages = records));
        Debug.Log($"DynamicUIManager: Caricati {_allExerciseAverages.Count} record di esercizio.");

        yield return StartCoroutine(LoadXmlAveragesCoroutine<ActivityAverage>("running_medie_per_ora_giorno.xml", "Average", records => _allRunningAverages = records));
        Debug.Log($"DynamicUIManager: Caricati {_allRunningAverages.Count} record di corsa.");

        yield return StartCoroutine(LoadXmlAveragesCoroutine<ActivityAverage>("walking_medie_per_ora_giorno.xml", "Average", records => _allWalkingAverages = records));
        Debug.Log($"DynamicUIManager: Caricati {_allWalkingAverages.Count} record di camminata.");

        Debug.Log("DynamicUIManager: Caricamento di tutti i record completato.");
        OnRecordsLoaded();
    }

    private IEnumerator LoadXmlAveragesCoroutine<T>(string filename, string elementName, System.Action<List<T>> onComplete) where T : new()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, filename);
        filePath = filePath.Replace("\\", "/");

        Debug.Log($"[DynamicUIManager] Tentativo di caricare file da: {filePath}");

        string xmlContent = "";

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            using (UnityWebRequest www = UnityWebRequest.Get(filePath))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    xmlContent = www.downloadHandler.text;
                    Debug.Log($"[DynamicUIManager] File XML '{filename}' caricato con successo via UnityWebRequest. Dimensione: {xmlContent.Length} caratteri.");
                }
                else
                {
                    Debug.LogError($"[DynamicUIManager] Errore UnityWebRequest nel caricare file XML '{filename}': {www.error} da: {filePath}");
                    onComplete?.Invoke(new List<T>());
                    yield break;
                }
            }
        }
        else
        {
            if (File.Exists(filePath))
            {
                xmlContent = File.ReadAllText(filePath);
                Debug.Log($"[DynamicUIManager] File XML '{filename}' caricato con successo da File.ReadAllText. Dimensione: {xmlContent.Length} caratteri.");
            }
            else
            {
                Debug.LogError($"[DynamicUIManager] File XML non trovato in Editor/Standalone: {filePath}");
                onComplete?.Invoke(new List<T>());
                yield break;
            }
        }

        if (!string.IsNullOrEmpty(xmlContent))
        {
            try
            {
                XDocument xmlDoc = XDocument.Parse(xmlContent);
                List<T> records = new List<T>();

                foreach (XElement element in xmlDoc.Descendants(elementName))
                {
                    T record = new T();
                    if (record is HeartRateAverage hra)
                    {
                        string dateAttr = element.Attribute("date")?.Value;
                        string hourAttr = element.Attribute("hour")?.Value;
                        string avgAttr = element.Attribute("average")?.Value;
                        string minAttr = element.Attribute("min")?.Value;
                        string maxAttr = element.Attribute("max")?.Value;

                        if (string.IsNullOrEmpty(dateAttr) || string.IsNullOrEmpty(hourAttr) || string.IsNullOrEmpty(avgAttr))
                        {
                            Debug.LogWarning($"[DynamicUIManager] Saltando un record di battito cardiaco con attributi essenziali mancanti: Date='{dateAttr}', Hour='{hourAttr}', Average='{avgAttr}'. Elemento XML: {element.ToString()}");
                            continue;
                        }

                        if (!float.TryParse(avgAttr, NumberStyles.Float, CultureInfo.InvariantCulture, out float averageValue))
                        {
                            Debug.LogWarning($"[DynamicUIManager] Impossibile parsare 'average' per HeartRateAverage all'ora {hourAttr} del {dateAttr}. Valore: '{avgAttr}'. Setting to 0.");
                            averageValue = 0;
                        }

                        hra.date = dateAttr;
                        hra.hour = int.Parse(hourAttr);
                        hra.average = averageValue;

                        if (!string.IsNullOrEmpty(minAttr) && float.TryParse(minAttr, NumberStyles.Float, CultureInfo.InvariantCulture, out float minBpmValue))
                        {
                            hra.MinBpm = minBpmValue;
                        }
                        else
                        {
                            hra.MinBpm = hra.average;
                        }

                        if (!string.IsNullOrEmpty(maxAttr) && float.TryParse(maxAttr, NumberStyles.Float, CultureInfo.InvariantCulture, out float maxBpmValue))
                        {
                            hra.MaxBpm = maxBpmValue;
                        }
                        else
                        {
                            hra.MaxBpm = hra.average;
                        }
                        records.Add(record);
                    }
                    else if (record is ActivityAverage aa)
                    {
                        string typeAttr = element.Attribute("type")?.Value;
                        string dateAttr = element.Attribute("date")?.Value;
                        string hourAttr = element.Attribute("hour")?.Value;
                        string valueAttr = element.Attribute("value")?.Value;

                        if (string.IsNullOrEmpty(typeAttr) || string.IsNullOrEmpty(dateAttr) || string.IsNullOrEmpty(hourAttr) || string.IsNullOrEmpty(valueAttr))
                        {
                            Debug.LogWarning($"[DynamicUIManager] Saltando un record di attività con attributi essenziali mancanti: Type='{typeAttr}', Date='{dateAttr}', Hour='{hourAttr}', Value='{valueAttr}'. Elemento XML: {element.ToString()}");
                            continue;
                        }

                        if (!float.TryParse(valueAttr, NumberStyles.Float, CultureInfo.InvariantCulture, out float valueParsed))
                        {
                            Debug.LogWarning($"[DynamicUIManager] Impossibile parsare 'value' per ActivityAverage all'ora {hourAttr} del {dateAttr}. Valore: '{valueAttr}'. Setting to 0.");
                            valueParsed = 0;
                        }

                        aa.type = typeAttr;
                        aa.date = dateAttr;
                        aa.hour = int.Parse(hourAttr);
                        aa.value = valueParsed;
                        records.Add(record);
                    }
                }
                onComplete?.Invoke(records);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicUIManager] Errore durante il parsing del file XML {filename}: {e.Message}\nContenuto XML che ha causato l'errore (prime 500 char): {xmlContent.Substring(0, Mathf.Min(xmlContent.Length, 500))}");
                onComplete?.Invoke(new List<T>());
            }
        }
        else
        {
            Debug.LogWarning($"[DynamicUIManager] Contenuto XML vuoto per {filename}. Nessun record caricato o deserializzato.");
            onComplete?.Invoke(new List<T>());
        }
    }

    void OnRecordsLoaded()
    {
        Debug.Log("DynamicUIManager: Inizio OnRecordsLoaded. Aggiorno dropdown anni.");
        List<int> yearsToExclude = new List<int> { 2019, 2020 };

        var allYearsFromRecords = new List<int>();
        allYearsFromRecords.AddRange(_allHeartRateAverages.Select(r => r.Date.Year));
        allYearsFromRecords.AddRange(_allExerciseAverages.Select(a => a.Date.Year));
        allYearsFromRecords.AddRange(_allRunningAverages.Select(a => a.Date.Year));
        allYearsFromRecords.AddRange(_allWalkingAverages.Select(a => a.Date.Year));

        List<int> distinctYearsBeforeFilter = allYearsFromRecords.Distinct().OrderBy(y => y).ToList();
        Debug.Log("DynamicUIManager: Anni distinti caricati dai file (prima del filtro): " + (distinctYearsBeforeFilter.Any() ? string.Join(", ", distinctYearsBeforeFilter) : "Nessuno"));

        List<int> yearsAfterFilter = distinctYearsBeforeFilter
                                                .Where(y => !yearsToExclude.Contains(y))
                                                .OrderBy(y => y)
                                                .ToList();

        Debug.Log("DynamicUIManager: Anni dopo l'applicazione del filtro (2019, 2020 esclusi): " + (yearsAfterFilter.Any() ? string.Join(", ", yearsAfterFilter) : "Nessuno"));

        _yearDropdown.choices.Clear();
        foreach (int year in yearsAfterFilter)
        {
            _yearDropdown.choices.Add(year.ToString());
        }

        // Imposta il valore iniziale su "-"
        _yearDropdown.SetValueWithoutNotify("-");
        _monthDropdown.SetValueWithoutNotify("-");
        _dayDropdown.SetValueWithoutNotify("-");

        // Aggiungi un listener al cambio di valore del dropdown Anno
        _yearDropdown.RegisterValueChangedCallback(evt => OnDateSelectionChanged());

        // Ora che tutti i dropdown hanno valori iniziali validi, chiama la funzione di aggiornamento.
        OnDateSelectionChanged();

        Debug.Log("DynamicUIManager: Fine OnRecordsLoaded.");
    }

    private void PopulateDayDropdown(DropdownField dayDropdown, int year, int month)
    {
        dayDropdown.choices.Clear();
        dayDropdown.choices.Add("-");

        if (month < 1 || month > 12 || year < 1)
        {
            dayDropdown.value = "-";
            Debug.LogWarning($"DynamicUIManager: Tentativo di popolare Dropdown Giorno con data invalida: Anno={year}, Mese={month}.");
            return;
        }

        int daysInMonth = DateTime.DaysInMonth(year, month);
        for (int i = 1; i <= daysInMonth; i++)
        {
            dayDropdown.choices.Add(i.ToString("00"));
        }

        string currentDayValue = dayDropdown.value;
        if (!string.IsNullOrEmpty(currentDayValue) && currentDayValue != "-" && int.TryParse(currentDayValue, out int selectedDay) && selectedDay <= daysInMonth)
        {
            dayDropdown.value = selectedDay.ToString("00");
        }
        else
        {
            dayDropdown.value = "-";
        }

        dayDropdown.SetValueWithoutNotify(dayDropdown.value);
        Debug.Log($"DynamicUIManager: Dropdown Giorno popolato per {year}-{month}. Giorno selezionato: {dayDropdown.value}");
    }

    private void OnDateSelectionChanged()
    {
        Debug.Log("DynamicUIManager: OnDateSelectionChanged chiamato.");
        int year, month, day;

        bool yearParsed = int.TryParse(_yearDropdown.value, out year) && _yearDropdown.value != "-";
        bool monthParsed = int.TryParse(_monthDropdown.value, out month) && _monthDropdown.value != "-";

        if (!yearParsed || !monthParsed)
        {
            PopulateDayDropdown(_dayDropdown, 0, 0);
            _dayDropdown.value = "-";
            _currentSelectedDate = DateTime.MinValue;
            Debug.Log("DynamicUIManager: Anno o Mese non validi. I dropdown Giorno e i grafici saranno svuotati.");
            PopulateChartsWithFilteredRecords(DateTime.MinValue);
            return;
        }

        PopulateDayDropdown(_dayDropdown, year, month);

        bool dayParsed = int.TryParse(_dayDropdown.value, out day) && _dayDropdown.value != "-";

        if (yearParsed && monthParsed && dayParsed &&
            year > 0 && month >= 1 && month <= 12 && day >= 1 && day <= DateTime.DaysInMonth(year, month))
        {
            _currentSelectedDate = new DateTime(year, month, day);
            Debug.Log($"DynamicUIManager: Data selezionata: {_currentSelectedDate.ToShortDateString()}");

            ChangeDropdownLabelColor(_yearDropdown);
            ChangeDropdownLabelColor(_monthDropdown);
            ChangeDropdownLabelColor(_dayDropdown);

            if (_centralContainer.style.display == DisplayStyle.Flex)
            {
                PopulateChartsWithFilteredRecords(_currentSelectedDate);
            }
            else
            {
                Debug.Log("DynamicUIManager: Menu completo non visibile. Non aggiorno i grafici. Data selezionata ma non mostrata.");
            }
        }
        else
        {
            Debug.LogWarning($"DynamicUIManager: Data selezionata incompleta o non valida: Anno={_yearDropdown.value}, Mese={_monthDropdown.value}, Giorno={_dayDropdown.value}. Non popolerò i grafici.");
            _currentSelectedDate = DateTime.MinValue;
            PopulateChartsWithFilteredRecords(DateTime.MinValue);

            ChangeDropdownLabelColor(_yearDropdown);
            ChangeDropdownLabelColor(_monthDropdown);
            ChangeDropdownLabelColor(_dayDropdown);
        }
    }

    private void PopulateChartsWithFilteredRecords(DateTime dateToFilter)
    {
        Debug.Log($"DynamicUIManager: Inizio PopulateChartsWithFilteredRecords per la data: {dateToFilter.ToShortDateString()}");

        if (dateToFilter == DateTime.MinValue)
        {
            Debug.Log("DynamicUIManager: Data di filtro non valida (DateTime.MinValue). Inizializzo grafici a vuoto.");
            if (_heartRateChart != null)
            {
                _heartRateChart.SetData(
                    new List<HeartRateAverage>(),
                    new List<ActivityAverage>(),
                    new List<ActivityAverage>(),
                    new List<ActivityAverage>(),
                    0f, 0f, 0f
                );
                _heartRateChart.HideDailyStatsPopup();
                _heartRateChart.HideActivityDetailPage();
            }
            return;
        }

        var filteredHeartRatesAverages = _allHeartRateAverages
            .Where(h => h.Date.Date == dateToFilter.Date)
            .OrderBy(h => h.hour)
            .ToList();

        Debug.Log($"DynamicUIManager: Trovati {filteredHeartRatesAverages.Count} record di battito cardiaco per la data {dateToFilter.ToShortDateString()}.");

        float minBpmDaily = 0f;
        float maxBpmDaily = 0f;
        float avgBpmDaily = 0f;

        if (filteredHeartRatesAverages.Any())
        {
            minBpmDaily = filteredHeartRatesAverages.Min(h => h.average);
            maxBpmDaily = filteredHeartRatesAverages.Max(h => h.average);
            avgBpmDaily = filteredHeartRatesAverages.Average(h => h.average);
        }
        else
        {
            Debug.LogWarning($"DynamicUIManager: Nessun dato di battito cardiaco filtrato per {dateToFilter.ToShortDateString()}. Le statistiche giornaliere saranno 0.");
        }
        Debug.Log($"DynamicUIManager: Statistiche BPM giornaliere calcolate (basate su medie orarie): Min={minBpmDaily:F2}, Max={maxBpmDaily:F2}, Avg={avgBpmDaily:F2}");

        var filteredExercise = _allExerciseAverages.Where(a => a.Date.Date == dateToFilter.Date).ToList();
        var filteredRunning = _allRunningAverages.Where(a => a.Date.Date == dateToFilter.Date).ToList();
        var filteredWalking = _allWalkingAverages.Where(a => a.Date.Date == dateToFilter.Date).ToList();

        Debug.Log($"DynamicUIManager: Trovati {filteredExercise.Count} record esercizio, {filteredRunning.Count} record corsa, {filteredWalking.Count} record camminata per la data {dateToFilter.ToShortDateString()}.");

        if (_heartRateChart != null)
        {
            _heartRateChart.SetData(
                filteredHeartRatesAverages,
                filteredExercise,
                filteredRunning,
                filteredWalking,
                minBpmDaily,
                maxBpmDaily,
                avgBpmDaily
            );
            Debug.Log("DynamicUIManager: Chiamato HeartRateChart.SetData con i dati filtrati.");
        }
        else
        {
            Debug.LogError("DynamicUIManager: _heartRateChart è NULL. Impossibile passare i dati.");
        }
        Debug.Log("DynamicUIManager: Fine PopulateChartsWithFilteredRecords.");
    }

    private string MapActivityType(string hkTypeIdentifier)
    {
        switch (hkTypeIdentifier)
        {
            case "HKQuantityTypeIdentifierAppleExerciseTime":
                return "Tempo di Esercizio";
            case "HKQuantityTypeIdentifierAppleWalkingSteadiness":
                return "Stabilità Camminata/Corsa";
            case "HKQuantityTypeIdentifierDistanceWalkingRunning":
                return "Distanza Camminata/Corsa";
            default:
                return hkTypeIdentifier;
        }
    }

    /// <summary>
    /// Controlla la visibilità dell'intero menu 2D (con dropdown e grafici).
    /// </summary>
    /// <param name="isVisible">True per mostrare, false per nascondere.</param>
    public void ToggleFullUIMenuVisibility(bool isVisible)
    {
        if (_centralContainer != null)
        {
            if (isVisible)
            {
                _centralContainer.style.display = DisplayStyle.Flex;
                _isFullUIMenuCurrentlyVisible = true; // Aggiorna lo stato di visibilità
                Debug.Log("DynamicUIManager: UI Centrale COMPLETA RESA VISIBILE.");
                // Se la UI completa è visibile, non vogliamo che l'AR display persista o sia visibile in background
                if (arHeartDisplayManager != null)
                {
                    arHeartDisplayManager.HideARDisplay();
                }
                _shouldARDisplayPersist = false; // Reset della persistenza quando si apre il menu completo

                // Assicurati che i dropdown siano popolati e i grafici aggiornati quando il menu diventa visibile
                if (!_hasFullUIMenuBeenShownInitially)
                {
                    OnRecordsLoaded(); // Ripopola gli anni se è la prima volta che si mostra il menu completo
                    _hasFullUIMenuBeenShownInitially = true;
                }
                // Chiamare OnDateSelectionChanged per assicurarsi che i grafici siano popolati con la data corrente
                // (o svuotati se la data non è valida)
                OnDateSelectionChanged();
            }
            else
            {
                _centralContainer.style.display = DisplayStyle.None;
                _isFullUIMenuCurrentlyVisible = false; // Aggiorna lo stato di visibilità
                Debug.Log("DynamicUIManager: UI Centrale COMPLETA RESA NASCOSTA.");

                // Quando il menu completo viene nascosto, se il target è tracciato
                // e la UI AR dovrebbe persistere, o se si torna indietro da una selezione di dati,
                // allora la UI AR dovrebbe riapparire.
                if (_isTargetCurrentlyTracked && _shouldARDisplayPersist)
                {
                    if (arHeartDisplayManager != null)
                    {
                        // Se c'è una data selezionata valida, ripristina la UI AR con quei dati
                        if (_currentSelectedDate != DateTime.MinValue)
                        {
                            var filteredHeartRatesAverages = _allHeartRateAverages
                                .Where(h => h.Date.Date == _currentSelectedDate.Date)
                                .OrderBy(h => h.hour)
                                .ToList();

                            float minBpmDaily = filteredHeartRatesAverages.Any() ? filteredHeartRatesAverages.Min(h => h.average) : 0f;
                            float maxBpmDaily = filteredHeartRatesAverages.Any() ? filteredHeartRatesAverages.Max(h => h.average) : 0f;
                            float avgBpmDaily = filteredHeartRatesAverages.Any() ? filteredHeartRatesAverages.Average(h => h.average) : 0f;

                            arHeartDisplayManager.UpdateARDisplay(minBpmDaily, maxBpmDaily, avgBpmDaily, _currentSelectedDate);
                            Debug.Log("DynamicUIManager: UI AR ripristinata con dati precedenti.");
                        }
                        else
                        {
                            arHeartDisplayManager.InitializeAndShowARDisplay();
                            Debug.Log("DynamicUIManager: UI AR ripristinata in modalità base.");
                        }
                    }
                }
            }
        }
    }
}