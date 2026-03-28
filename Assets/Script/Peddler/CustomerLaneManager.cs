using System;
using System.Collections.Generic;

public class CustomerLaneManager
{
    private bool[] isActiveLanes;


    public CustomerLaneManager(int laneSize)
    {
        this.isActiveLanes = new bool[laneSize];
    }

    public void ToggleCustomerLane(int laneNum)
    {
        isActiveLanes[laneNum] = !isActiveLanes[laneNum];
    }    

    public int GetEmptyLane()
    {
        List<int> emptyLane =  new List<int>();
        //—”—v‘f‚ğ•t‚¯‚½‚¢‚©‚à
        for(int i = 0; i < isActiveLanes.Length; i ++)
        {
            if(!isActiveLanes[i])
            {
                emptyLane.Add(i);
            }
        }        
        if (emptyLane.Count == 0) return -1;

        Random rand = new Random();
        int selection = rand.Next(emptyLane.Count);
        
        return emptyLane[selection];
    }   
}