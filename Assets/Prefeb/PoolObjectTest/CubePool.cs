using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utopia.Core.Services;

public class CubePool : MonoBehaviour
{
    [ServiceInject]
    private IObjectPoolService _objectPoolService;

    public GameObject cube;

    public Transform tran;

    private void Start()
    {
        if (ServiceLocatorProvider.Global.Locator != null)
        {
            ServiceLocatorProvider.Global.Locator.Inject(this);
        }

        _objectPoolService.CreatePool("Dirt", cube);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            List<GameObject> obj = _objectPoolService.GetMultipleFromPool("Dirt",10, tran.position, tran.transform.rotation);

            if (obj != null)
            {
                _objectPoolService.ReturnMultipleToPool("Dirt", obj, 1);
            } 
        }
    }
}
