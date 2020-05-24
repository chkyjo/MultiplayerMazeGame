using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PyramidController : NetworkBehaviour{

    [SerializeField]
    private GameObject keyPrefab;

    bool topHasKey = false;
    bool rightHasKey = false;
    bool bottomHasKey = false;
    bool leftHasKey = false;

    public void AddKeyToTop() {
        if (topHasKey) {
            Debug.Log("[PyramidController] Key already in top slot");
            return;
        }

        GameObject keyInstance = Instantiate(keyPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(keyInstance);
        //key origin puts model in the bottom, rotate 180 to bring it to top
        keyInstance.transform.Rotate(new Vector3(0, 90, 0));

        topHasKey = true;

        CheckKeys();
    }

    public void AddKeyToRight() {
        if (rightHasKey) {
            Debug.Log("[PyramidController] Key already in right slot");
            return;
        }

        GameObject keyInstance = Instantiate(keyPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(keyInstance);
        keyInstance.transform.Rotate(new Vector3(0, 180, 0));

        rightHasKey = true;

        CheckKeys();
    }

    public void AddKeyToBottom() {
        if (bottomHasKey) {
            Debug.Log("[PyramidController] Key already in bottom slot");
            return;
        }

        GameObject keyInstance = Instantiate(keyPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(keyInstance);
        keyInstance.transform.Rotate(new Vector3(0, -90, 0));

        bottomHasKey = true;

        CheckKeys();
    }

    public void AddKeyToLeft() {
        if (leftHasKey) {
            Debug.Log("[PyramidController] Key already in left slot");
            return;
        }

        GameObject keyInstance = Instantiate(keyPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(keyInstance);

        leftHasKey = true;

        CheckKeys();
    }

    void CheckKeys() {
        if(topHasKey && rightHasKey && bottomHasKey && leftHasKey) {
            int exitDirection = Random.Range(0, 4); //backleft, backright, frontleft, frontright

            // You can re-use this block between calls rather than constructing a new one each time.
            var block = new MaterialPropertyBlock();

            // You can look up the property by ID instead of the string to be more efficient.
            block.SetColor("_BaseColor", Color.blue);

            // You can cache a reference to the renderer to avoid searching for it.
            transform.GetChild(exitDirection).GetComponent<Renderer>().SetPropertyBlock(block);

            MazeGenerator.Instance.CreateExit(exitDirection);
        }
    }
}
