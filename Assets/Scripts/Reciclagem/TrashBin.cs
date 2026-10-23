using UnityEngine;
using System.Collections;

public class TrashBin : MonoBehaviour
{
    public TrashType AcceptedType;
    public void TryReceive(TrashItem item)
    {
        if (item.Type == AcceptedType)
        {
            Accept(item);
        }
        else
        {
            StartCoroutine(Reject(item));
        }
    }

    void Accept(TrashItem item)
    {
        SoundManager.Instance?.PlaySound2D("recycle_correct");
        item.gameObject.SetActive(false);
        TrashItem[] remaining =
            FindObjectsByType<TrashItem>(FindObjectsSortMode.None);
        int activeCount = 0;
        foreach (var t in remaining)
        {
            if (t.gameObject.activeSelf)
            {
                activeCount++;
            }
        }
        if (activeCount == 0)
        {
            GameEvents.LevelCompleteMethod();
        }
    }

    IEnumerator Reject(TrashItem item)
    {
        SoundManager.Instance?.PlaySound2D("recycle_wrong");
        Vector2 startPos = item.transform.position;
        Vector2 targetPos = item.GetComponent<Draggable>().OriginalPosition;
        float duration = 0.4f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float arc = Mathf.Sin(t * Mathf.PI) * 1.5f;
            Vector2 pos = Vector2.Lerp(startPos, targetPos, t);
            pos.y += arc;
            item.transform.position = pos;
            yield return null;
        }
        item.transform.position = targetPos;
    }
}