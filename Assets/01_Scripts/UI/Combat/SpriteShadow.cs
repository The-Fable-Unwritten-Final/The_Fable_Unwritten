
using UnityEngine;

public class SpriteShadow : MonoBehaviour
{
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private SpriteRenderer shadow;

    void LateUpdate()
    {
        shadow.sprite = body.sprite;
        shadow.flipX = body.flipX;
        shadow.flipY = body.flipY;
    }
}
