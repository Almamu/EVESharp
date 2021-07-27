def Load(self, node):
    listentry.Generic.Load(self, node)
    self.sr.node = node
    data = node
    self.corporationID = data.corporationID
    self.sr.corpicon.state = uix.UI_NORMAL
    uix.Flush(self.sr.corpicon)
    corplogo = uix.GetCorpLogo(data.corporationID, 0, clipped=1, align=uix.UI_ALCLIENT)
    self.sr.corpicon.children.append(corplogo)
    self.sr.infoicon.OnClick = self.ShowInfo
    self.sr.infoicon.state = uix.UI_NORMAL
    self.joinleaveBtn.state = uix.UI_NORMAL
    
    if eve.session.corpid == self.corporationID:
        self.joinleaveBtn.state = uix.UI_HIDDEN

    self.joinleaveBtn.SetLabel(mls.UI_CMD_APPLY)
    return
