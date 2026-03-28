using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class CustomerCreator : MonoBehaviour
{
    [SerializeField] private Vector3[] leavePositions;
    [SerializeField] private Vector3[] startPositions;
    [SerializeField] private ObjectPool pool;

    [SerializeField] private float moveToStoreTime;
    [SerializeField] private int laneSize;

    private CustomerController customer;

    private CustomerLaneManager laneManager;

    private float timer;

    private float createInterval = 5f;

    private int customerSize = 30;

    private void Start()
    {
        laneManager = new CustomerLaneManager(laneSize);
        StartCreate();
    }

    private void Update()
    {
        
    }

    private async UniTaskVoid StartCreate()
    {
        Debug.Log("StartCreate");
        CreateCustomer(UnityEngine.Random.Range(1,laneSize));
        for(int i = 0; i < customerSize; i++)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3f));
            int emptyLane = laneManager.GetEmptyLane();
            if(emptyLane != -1)
            {
                Debug.Log("CreateCustomer");
                CreateCustomer(emptyLane);
            }
        }
        

    }

    private void CreateCustomer(int laneNum)
    {
        Debug.Log("LanNum : " + laneNum.ToString());
        laneManager.ToggleCustomerLane(laneNum);
        Debug.Log("Initialize Lane");
        PooledObject customerObj = pool.GetPooledObject();
        customerObj.transform.position = startPositions[laneNum];
        customer = customerObj.GetComponent<CustomerController>();
        customer.Data.moveToStoreTime = 100f;
        customer.Data.leavePos = leavePositions[laneNum];
        //遷移時間を書き加える
        //移動スピードを設定
        //whisiListを設定

    }
}
