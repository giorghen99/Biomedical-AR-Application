using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ARHeartDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject _arDisplayParent; // Il parent che contiene cuore, polmoni e tutti i testi/bottoni AR
    [SerializeField] private TextMeshProUGUI _minBpmText;
    [SerializeField] private TextMeshProUGUI _maxBpmText;
    [SerializeField] private TextMeshProUGUI _avgBpmText;
    [SerializeField] private TextMeshProUGUI _dateTextAR;

    // Riferimento al bottone per mostrare il menu completo dalla vista AR
    [SerializeField] private Button _showMenuButtonAR; // Rinominato per chiarezza nel contesto

    // Nuovo riferimento a un GameObject che contiene tutti i TextMeshPro per i BPM e la data
    [SerializeField] private GameObject _bpmInfoContainer; // Un GameObject vuoto che raggruppa tutti i TextMeshPro

    private DynamicUIManager _dynamicUIManager;

    private Color32 BPM_NORMAL_COLOR = new Color32(0, 200, 0, 255);    // Verde
    private Color32 BPM_HIGH_COLOR = new Color32(255, 0, 0, 255);     // Rosso
    private Color32 BPM_LOW_COLOR = new Color32(255, 255, 0, 255);     // Giallo
    private Color32 BPM_NO_DATA_COLOR = new Color32(100, 100, 100, 255); // Grigio scuro

    void Awake()
    {
        // Assicurati che l'oggetto AR sia inizialmente nascosto
        HideARDisplay();

        // Registra il listener per il bottone "Mostra Dettagli"
        if (_showMenuButtonAR != null)
        {
            _showMenuButtonAR.onClick.AddListener(OnShowMenuClicked);
            // Imposta il testo del bottone all'Awake (puoi anche farlo nell'editor)
            TextMeshProUGUI buttonText = _showMenuButtonAR.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Mostra Dettagli";
            }
        }
        else
        {
            Debug.LogWarning("ARHeartDisplayManager: ShowMenuButtonAR non assegnato nell'Inspector.");
        }

        // Verifica che il contenitore info BPM sia assegnato
        if (_bpmInfoContainer == null)
        {
            Debug.LogWarning("ARHeartDisplayManager: _bpmInfoContainer non assegnato nell'Inspector. Le informazioni BPM potrebbero non nascondersi correttamente.");
        }

        // Inizialmente nascondi il contenitore delle info BPM
        if (_bpmInfoContainer != null)
        {
            _bpmInfoContainer.SetActive(false);
        }
    }

    public void SetDynamicUIManager(DynamicUIManager manager)
    {
        _dynamicUIManager = manager;
    }

    /// <summary>
    /// Inizializza e mostra il display AR con solo cuore/polmoni e il bottone.
    /// Questo viene chiamato quando il target è trovato per la prima volta.
    /// </summary>
    public void InitializeAndShowARDisplay()
    {
        if (_arDisplayParent != null)
        {
            _arDisplayParent.SetActive(true);
        }

        // Nascondi sempre le informazioni BPM e la data quando si inizializza il display AR
        if (_bpmInfoContainer != null)
        {
            _bpmInfoContainer.SetActive(false);
        }

        // Assicurati che il bottone "Mostra Dettagli" sia visibile
        if (_showMenuButtonAR != null)
        {
            _showMenuButtonAR.gameObject.SetActive(true);
            TextMeshProUGUI buttonText = _showMenuButtonAR.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Mostra Dettagli"; // Testo iniziale
            }
        }

        Debug.Log("AR Display Initialized and Shown (only models and 'Show Details' button).");
    }

    /// <summary>
    /// Aggiorna il display AR con le statistiche BPM e la data selezionata.
    /// Questo viene chiamato quando si clicca "Mostra in AR" dal menu completo.
    /// </summary>
    public void UpdateARDisplay(float minBpm, float maxBpm, float avgBpm, DateTime selectedDate)
    {
        if (_arDisplayParent != null)
        {
            _arDisplayParent.SetActive(true);
        }

        // Mostra le informazioni BPM e la data
        if (_bpmInfoContainer != null)
        {
            _bpmInfoContainer.SetActive(true);
        }

        // --- INIZIO MODIFICA LOGICA VISUALIZZAZIONE BPM ---
        // Controlla se i valori Min, Max e Medio sono identici
        if (Mathf.Approximately(minBpm, maxBpm) && Mathf.Approximately(maxBpm, avgBpm) && minBpm > 0) // Aggiunto controllo > 0 per dati validi
        {
            // Se sono identici e validi, mostra solo il valore medio come "BPM: X"
            if (_avgBpmText != null)
            {
                _avgBpmText.text = $"BPM: {avgBpm:F0}";
                _avgBpmText.color = GetBpmColor(avgBpm);
                _avgBpmText.gameObject.SetActive(true); // Assicurati che sia attivo
            }
            if (_minBpmText != null)
            {
                _minBpmText.gameObject.SetActive(false); // Nascondi Min
            }
            if (_maxBpmText != null)
            {
                _maxBpmText.gameObject.SetActive(false); // Nascondi Max
            }
        }
        else // Se i valori sono diversi, o se sono tutti zero, mostra Min, Max, Medio
        {
            if (_minBpmText != null)
            {
                _minBpmText.text = $"Min: {minBpm:F0}";
                _minBpmText.color = GetBpmColor(minBpm);
                _minBpmText.gameObject.SetActive(true); // Assicurati che sia attivo
            }
            if (_maxBpmText != null)
            {
                _maxBpmText.text = $"Max: {maxBpm:F0}";
                _maxBpmText.color = GetBpmColor(maxBpm);
                _maxBpmText.gameObject.SetActive(true); // Assicurati che sia attivo
            }
            if (_avgBpmText != null)
            {
                _avgBpmText.text = $"Medio: {avgBpm:F0}";
                _avgBpmText.color = GetBpmColor(avgBpm);
                _avgBpmText.gameObject.SetActive(true); // Assicurati che sia attivo
            }

            // Caso specifico: se non ci sono dati validi (tutti 0 o negativi)
            if (minBpm <= 0 && maxBpm <= 0 && avgBpm <= 0)
            {
                if (_avgBpmText != null)
                {
                    _avgBpmText.text = "BPM: --"; // Messaggio per nessun dato
                    _avgBpmText.color = BPM_NO_DATA_COLOR;
                    _avgBpmText.gameObject.SetActive(true);
                }
                if (_minBpmText != null) _minBpmText.gameObject.SetActive(false);
                if (_maxBpmText != null) _maxBpmText.gameObject.SetActive(false);
            }
        }
        // --- FINE MODIFICA LOGICA VISUALIZZAZIONE BPM ---


        // *** BLOCCO DATA (VERIFICA O AGGIUNGI QUESTO) ***
        // Questo blocco rimane invariato e va bene così
        if (_dateTextAR != null)
        {
            _dateTextAR.text = selectedDate.ToString("DATA: dd/MM/yyyy"); // Formato Giorno/Mese/Anno
            _dateTextAR.color = new Color32(240, 240, 240, 255);     // Colore quasi bianco
            _dateTextAR.gameObject.SetActive(true); // Assicurati che la data sia attiva
        }
        else
        {
            Debug.LogWarning("ARHeartDisplayManager: _dateTextAR non assegnato. La data non verrà visualizzata in AR.");
        }

        // Assicurati che il bottone "Mostra Dettagli" sia visibile
        if (_showMenuButtonAR != null)
        {
            _showMenuButtonAR.gameObject.SetActive(true);
            TextMeshProUGUI buttonText = _showMenuButtonAR.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Mostra Dettagli";
                buttonText.color = new Color32(50, 50, 50, 255); // O il colore che hai scelto per il bottone
            }
        }

        Debug.Log($"AR Display Updated: Min {minBpm}, Max {maxBpm}, Avg {avgBpm}, Date {selectedDate.ToShortDateString()}");
    }

    public void HideARDisplay()
    {
        if (_arDisplayParent != null)
        {
            _arDisplayParent.SetActive(false);
        }
        // Nascondi anche il contenitore delle info BPM e il bottone quando il display AR è nascosto completamente
        if (_bpmInfoContainer != null)
        {
            _bpmInfoContainer.SetActive(false);
        }
        if (_showMenuButtonAR != null)
        {
            _showMenuButtonAR.gameObject.SetActive(false);
        }
        Debug.Log("AR Display Hidden.");
    }

    private Color32 GetBpmColor(float bpm)
    {
        if (bpm <= 0) return BPM_NO_DATA_COLOR;
        if (bpm < 60f) return BPM_LOW_COLOR;
        if (bpm > 100f) return BPM_HIGH_COLOR;
        return BPM_NORMAL_COLOR;
    }

    private void OnShowMenuClicked()
    {
        Debug.Log("AR 'Show Details' button clicked. Hiding AR display and showing full UI menu.");
        HideARDisplay(); // Nasconde la visualizzazione AR completa (modelli + testi + bottone)

        if (_dynamicUIManager != null)
        {
            _dynamicUIManager.ToggleFullUIMenuVisibility(true); // <-- Corretto: usa _dynamicUIManager
        }
        else
        {
            Debug.LogWarning("ARHeartDisplayManager: DynamicUIManager reference is missing for show menu button.");
        }
    }
}