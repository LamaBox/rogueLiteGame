using UnityEngine;

public class BackgroundHolder : MonoBehaviour
{
    public GameObject[] leftPictures;
    public GameObject[] centerPictures;
    public GameObject[] rightPictures;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CenterPictures()
    {
        foreach (var left in leftPictures)
        {
            left.transform.position = new Vector3(-27.2f,  left.transform.position.y, left.transform.position.z);
        }
        foreach (var center in centerPictures)
        {
            center.transform.position = new Vector3(0, center.transform.position.y, center.transform.position.z);
        }
        foreach (var right in rightPictures)
        {
            right.transform.position = new Vector3(27.2f, right.transform.position.y, right.transform.position.z);
        }
    }
}
