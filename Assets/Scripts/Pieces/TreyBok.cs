using UnityEngine;
using UnityEngine.UI;

public class Treybok : BasePiece
{
    public override void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        // Base setup
        base.Setup(newTeamColor, newSpriteColor, newPieceManager);

        // Queen stuff
        mMovement = new Vector3Int(0, 0, 1);
        Sprite[] sprites = Resources.LoadAll<Sprite>("Trey 1");

        foreach (Sprite s in sprites)
        {
            if (s.name == "Trey 1_1")
            {
                GetComponent<Image>().sprite = s;
                break;
            }
        }
    }
}
