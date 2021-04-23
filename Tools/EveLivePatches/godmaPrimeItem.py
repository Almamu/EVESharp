def OnGodmaPrimeItem(self, locationID, row):
    stateMgr = self.GetStateManager()
    stateMgr.OnGodmaPrimeItem(locationID, row)
    item = row.invItem
    sm.ScatterEvent('OnItemChange', item, {const.ixLocationID: None, const.ixFlag: None})
