using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        MOVES
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        LOADING,
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;


    private BoardController m_boardController;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    private ObjectPool<Cell> m_cellPool;
    private ItemViewPool m_itemViewPool;
    private Coroutine m_cellPoolWarmup;
    private bool m_loadingLevel;
    private eLevelMode m_levelMode;
    private Coroutine m_loadLevelCoroutine;
    private Coroutine m_gameOverCoroutine;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        Cell cellPrefab = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND).GetComponent<Cell>();
        m_cellPool = new ObjectPool<Cell>(cellPrefab, transform);
        m_itemViewPool = new ItemViewPool(transform);
        m_cellPoolWarmup = StartCoroutine(m_cellPool.Prewarm(m_gameSettings.BoardSizeX * m_gameSettings.BoardSizeY));

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    internal void SetState(eStateGame state)
    {
        if (m_loadingLevel && (state == eStateGame.PAUSE || state == eStateGame.GAME_STARTED)) return;
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        if (m_loadingLevel || m_boardController != null) return;
        m_levelMode = mode;
        m_loadingLevel = true;
        SetState(eStateGame.LOADING);
        m_loadLevelCoroutine = StartCoroutine(LoadLevelCoroutine(mode));
    }

    public void RestartLevel()
    {
        if (m_loadingLevel || m_boardController == null) return;

        ClearLevel();
        LoadLevel(m_levelMode);
    }

    private IEnumerator LoadLevelCoroutine(eLevelMode mode)
    {
        // Wait if Play was clicked before the menu finished prewarming the cells.
        yield return m_cellPoolWarmup;

        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        yield return m_boardController.StartGame(this, m_gameSettings, m_cellPool, m_itemViewPool);

        if (mode == eLevelMode.MOVES)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController);
        }
        else if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelTime, m_uiMenu.GetLevelConditionView(), this);
        }

        m_levelCondition.ConditionCompleteEvent += GameOver;

        m_loadingLevel = false;
        m_loadLevelCoroutine = null;
        SetState(eStateGame.GAME_STARTED);
    }

    public void GameOver()
    {
        if (m_loadingLevel || m_boardController == null || m_gameOverCoroutine != null || State == eStateGame.GAME_OVER) return;
        m_gameOverCoroutine = StartCoroutine(WaitBoardController());
    }

    internal void ClearLevel()
    {
        if (m_loadLevelCoroutine != null)
        {
            StopCoroutine(m_loadLevelCoroutine);
            m_loadLevelCoroutine = null;
        }
        if (m_gameOverCoroutine != null)
        {
            StopCoroutine(m_gameOverCoroutine);
            m_gameOverCoroutine = null;
        }
        m_loadingLevel = false;
        ClearLevelCondition();

        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }

    private IEnumerator WaitBoardController()
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = eStateGame.GAME_OVER;
        m_gameOverCoroutine = null;
        ClearLevelCondition();
    }

    private void ClearLevelCondition()
    {
        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= GameOver;
            m_levelCondition.enabled = false;
            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }
}
