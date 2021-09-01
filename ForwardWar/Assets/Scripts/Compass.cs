using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Compass : MonoBehaviour
{
    const float MaximunSprite = 20f;
    public GameObject iconPrefab;
    List<QuestMarker> questMarkers = new List<QuestMarker>();
    List<float> MarkersDistance = new List<float>();

    public RawImage compassImage;
    public Transform player;

    float compassUnit;

    public QuestMarker[] Markers;
    private float shortDist = float.MaxValue;
    private int shortInex;
    private void Start()
    {
        compassUnit = compassImage.rectTransform.rect.width / 360f;

        foreach(QuestMarker q in Markers)
        {
            AddQuestMarker(q);
        }
    }
    private void Update()
    {
        compassImage.uvRect = new Rect(player.localEulerAngles.y/360f, 0f, 1f, 1f);

        for (int i = 0; i < questMarkers.Count; ++i)
        {
            Markers[i].image.rectTransform.anchoredPosition = GetPosOnCompass(Markers[i]);

            float distance = Vector3.Distance(Markers[i].transform.position, player.position);
            float size = (1 - distance / (distance + distance)) * MaximunSprite;
            Markers[i].image.rectTransform.sizeDelta = new Vector2(size, size);
            MarkersDistance[i] = distance;
        }

        for (int i = 0; i < MarkersDistance.Count; ++i)
        {
            if (shortDist > MarkersDistance[i])
            {
                shortDist = MarkersDistance[i];
                shortInex = i;
                Debug.Log($"shortTrack Update: {i}");
            }
        }

        if (Input.GetKey(KeyCode.P))
        {
            player.GetComponent<PlayerMove>().FindWay(CloseObject());
        }
        Debug.Log(CloseObject());
    }

    public void AddQuestMarker (QuestMarker marker)
    {
        GameObject newMarker = Instantiate(iconPrefab, compassImage.transform);
        marker.image = newMarker.GetComponent<Image>();
        marker.image.sprite = marker.icon;

        questMarkers.Add(marker);
        MarkersDistance.Add(0);
    }

    public void RemoveQuestMarker(int index)
    {
        questMarkers.RemoveAt(index);
        MarkersDistance.RemoveAt(index);
    }

    public Vector3 CloseObject()
    {
        return Markers[shortInex].transform.position;
    }

    Vector2 GetPosOnCompass (QuestMarker marker)
    {
        Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector2 playerFwd = new Vector2(player.transform.forward.x, player.transform.forward.z);

        float angle = Vector2.SignedAngle(marker.position - playerPos, playerFwd);

        return new Vector2(compassUnit * angle, -7.5f);
    }
}
