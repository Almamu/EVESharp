def OnAllianceMemberChanged(self, allianceID, corporationID, change):
    def __GetLabel(self, corporationID):
        member = sm.GetService('alliance').GetMembers()[corporationID]
        corpName = cfg.eveowners.Get(corporationID).ownerName
        chosenExecutor = member.chosenExecutorID
        if member.chosenExecutorID is None:
            chosenExecutor = mls.UI_CORP_SECRET
        else:
            chosenExecutor = cfg.eveowners.Get(member.chosenExecutorID).ownerName
        return '<t>%s<t>%s' % (corpName, chosenExecutor)

    uix.LogInfo('OnAllianceMemberChanged allianceID', allianceID, 'corporationID', corporationID, 'change', change)
    if eve.session.allianceid != allianceID:
        return
    if self.sr.scroll is None:
        uix.LogInfo('OnAllianceMemberChanged no scroll')
        return
    bAdd = 1
    bRemove = 1
    for (old, new) in change.itervalues():
        if old is not None:
            bAdd = 0
        if new is not None:
            bRemove = 0

    if bAdd and bRemove:
        raise RuntimeError('members::OnAllianceMemberChanged WTF')
    if bAdd:
        uix.LogInfo('OnAllianceMemberChanged adding member')
        member = sm.GetService('alliance').GetMembers()[corporationID]
        self.SetHint()
        scrolllist = []
        self.__AddToList(member, scrolllist)
        if len(self.sr.scroll.sr.headers) > 0:
            self.sr.scroll.AddEntry(-1, scrolllist[0], update=1)
        else:
            self.sr.scroll.Load(19, scrolllist, headers=self.sr.headers)
    elif bRemove:
        uix.LogInfo('OnAllianceMemberChanged removing member')
        entry = self.GetEntry(allianceID, corporationID)
        if entry is not None:
            self.sr.scroll.RemoveEntries([entry])
        else:
            uix.LogWarn('OnAllianceMemberChanged member not found')
    else:
        uix.LogInfo('OnAllianceMemberChanged updating member')
        entry = self.GetEntry(allianceID, corporationID)
        if entry is None:
            uix.LogWarn('OnAllianceMemberChanged member not found')
        if entry is not None:
            label = __GetLabel(self, corporationID)
            entry.panel.sr.node.label = label
            entry.panel.sr.label.text = label
    return
