using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class GameController : MonoBehaviour
{
    public enum MassState
    {
        DEFAULT = -1,
        WHITE = 0,
        BLACK,
        Count,
    }
    private class MassObject
    {
        public MassState State = MassState.DEFAULT;
        public GameObject Stone = default;
    }
    private MassObject[,] _massObjects = new MassObject[8, 8];

    [SerializeField]
    private GameObject _stonePrefab = default;
    [SerializeField]
    private GameObject _highlightPrefab = default;
    [SerializeField]
    private TextMeshProUGUI _turnText = default;
    private bool _isPlayerTurn = true;
    private List<Vector2Int> _canPutMass = new List<Vector2Int>(16);
    private List<GameObject> _highlightObjects = new List<GameObject>();
    void Start()
    {
        PutStone(new Vector2Int(3, 3), MassState.WHITE);
        PutStone(new Vector2Int(4, 3), MassState.BLACK);
        PutStone(new Vector2Int(3, 4), MassState.BLACK);
        PutStone(new Vector2Int(4, 4), MassState.WHITE);
        UpdateCanPut(); // 初期状態でのハイライトを表示
        UpdateTurnText(); // 初期状態でのターン表示
    }
    void Update()
    {
        if (!_isPlayerTurn)
        {
            var r = UnityEngine.Random.Range(0, _canPutMass.Count);
            var color = MassState.BLACK;
            if (CanPutStone(_canPutMass[r], color))
            {
                PutStone(_canPutMass[r], color);
                _isPlayerTurn = !_isPlayerTurn;
                UpdateCanPut();
                UpdateTurnText();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit))
                {
                    var position = hit.point;
                    var massPosition = new Vector2Int
                    (
                        Mathf.RoundToInt((position.x - 0.55f) / 1.1f) + 4,
                        3 - Mathf.RoundToInt((position.z - 0.55f) / 1.1f)
                    );

                    if (!IsInBoard(massPosition)) return;
                    var color = _isPlayerTurn ? MassState.WHITE : MassState.BLACK;
                    if (CanPutStone(massPosition, color))
                    {
                        PutStone(massPosition, color);
                        _isPlayerTurn = !_isPlayerTurn;
                        UpdateCanPut();
                        UpdateTurnText();
                    }
                }
            }
        }
    }

    private bool CanPutStone(Vector2Int position, MassState color)
    {
        if (_massObjects[position.y, position.x] != default) return false;
        return CanReverse(position, color);
    }
    private bool CanReverse(Vector2Int position, MassState color)
    {
        if (CheckReverseDirection(position, color, new Vector2Int(0, -1))) return true;
        if (CheckReverseDirection(position, color, new Vector2Int(0, 1))) return true;
        if (CheckReverseDirection(position, color, new Vector2Int(-1, 0))) return true;
        if (CheckReverseDirection(position, color, new Vector2Int(1, 0))) return true;
        if (CheckReverseDirection(position, color, new Vector2Int(1, -1))) return true;
        if (CheckReverseDirection(position, color, new Vector2Int(1, 1))) return true;
        if (CheckReverseDirection(position, color, new Vector2Int(-1, -1))) return true;
        if (CheckReverseDirection(position, color, new Vector2Int(-1, 1))) return true;
        return false;
    }
    private bool CheckReverseDirection(Vector2Int position, MassState color, Vector2Int direction)
    {
        int count = 0;
        while (true)
        {
            var target = position + direction * (count + 1);
            if (!IsInBoard(target) || _massObjects[target.y, target.x] == default)
            {
                return false;
            }
            if (_massObjects[target.y, target.x]?.State == color)
            {
                return count > 0;
            }
            count++;
        }
    }

    private void PutStone(Vector2Int position, MassState color)
    {
        var go = Instantiate(_stonePrefab,
                            new Vector3(position.x * 1.1f - 3.85f, 0.1f, -position.y * 1.1f + 3.85f),
                            Quaternion.Euler(0f, 0f, color == MassState.WHITE ? 0f : 180f));
        _massObjects[position.y, position.x] = new MassObject
        {
            State = color,
            Stone = go,
        };
        Reverse(position, color, new Vector2Int(0, -1));
        Reverse(position, color, new Vector2Int(0, 1));
        Reverse(position, color, new Vector2Int(-1, 0));
        Reverse(position, color, new Vector2Int(1, 0));
        Reverse(position, color, new Vector2Int(1, -1));
        Reverse(position, color, new Vector2Int(1, 1));
        Reverse(position, color, new Vector2Int(-1, -1));
        Reverse(position, color, new Vector2Int(-1, 1));
    }
    private void Reverse(Vector2Int position, MassState color, Vector2Int direction)
    {
        var target = Vector2Int.zero;
        int count = 0;
        while (true)
        {
            target = position + direction * (count + 1);
            if (!IsInBoard(target) || _massObjects[target.y, target.x] == default)
            {
                count = -1;
                break;
            }
            if (_massObjects[target.y, target.x]?.State == color)
            {
                break;
            }
            count++;
        }
        for (int i = 0; i < count; i++)
        {
            target = position + direction * (i + 1);
            _massObjects[target.y, target.x].State = color;
            //_massObjects[target.y, target.x].Stone.transform.Rotate(0f, 0f, 180f);
            _massObjects[target.y, target.x].Stone.transform.DORotate
            (
                new Vector3(0f, 0f, _isPlayerTurn ? 0f : 180f),
                0.2f
            );
        }
    }
    private bool IsInBoard(Vector2Int mass)
    {
        return mass.x >= 0 && mass.y >= 0 && mass.x < 8 && mass.y < 8;
    }
    private void UpdateTurnText()
    {
        _turnText.text = _isPlayerTurn ? "あなたのターン" : "AIのターン";
    }
    private void UpdateCanPut()
    {
        _canPutMass.Clear();
        // ハイライトを全て削除
        foreach (var highlight in _highlightObjects)
        {
            Destroy(highlight);
        }
        _highlightObjects.Clear();

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (_massObjects[y, x] == default)
                {
                    if (CanReverse(new Vector2Int(x, y), _isPlayerTurn ? MassState.WHITE : MassState.BLACK))
                    {
                        _canPutMass.Add(new Vector2Int(x, y));
                        // ハイライトを追加
                        var highlight = Instantiate(_highlightPrefab,
                            new Vector3(x * 1.1f - 3.85f, 0.01f, -y * 1.1f + 3.85f),
                            Quaternion.Euler(90f, 0f, 0f));
                        _highlightObjects.Add(highlight);
                    }
                }
            }
        }
    }
}
