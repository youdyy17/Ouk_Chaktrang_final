using UnityEngine;

public class GameStatusController : MonoBehaviour
{
    public PieceManager pieceManager;
    public StatusOverlay statusOverlay;

    private void OnEnable()
    {
        if (pieceManager == null) return;
        pieceManager.OnCheck += HandleCheck;
        pieceManager.OnCheckmate += HandleCheckmate;
        pieceManager.OnCheckmateWithWinner += HandleCheckmateWithWinner;
        pieceManager.OnStalemate += HandleStalemate;
        pieceManager.OnClearStatus += HandleClear;
    }

    private void OnDisable()
    {
        if (pieceManager == null) return;
        pieceManager.OnCheck -= HandleCheck;
        pieceManager.OnCheckmate -= HandleCheckmate;
        pieceManager.OnCheckmateWithWinner -= HandleCheckmateWithWinner;
        pieceManager.OnStalemate -= HandleStalemate;
        pieceManager.OnClearStatus -= HandleClear;
    }

    private void HandleCheck() { statusOverlay?.ShowCheck(); }
    private void HandleCheckmate() { statusOverlay?.ShowCheckmate(); }
    private void HandleCheckmateWithWinner(Color winnerColor) { statusOverlay?.ShowWin(winnerColor); }
    private void HandleStalemate() { statusOverlay?.ShowDraw(); }
    private void HandleClear() { statusOverlay?.HideAll(); }
}
