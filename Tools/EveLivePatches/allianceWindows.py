def LoadViewClass(self, tabName):
    uix.Flush(self.sr.wndViewParent)
    if tabName == 'alliances_home':
        self.sr.currentView = form.AlliancesHome(self.sr.wndViewParent)
    elif tabName == 'alliances_rankings':
        self.sr.currentView = form.AlliancesRankings(self.sr.wndViewParent)
    elif tabName == 'alliances_applications':
        self.sr.currentView = form.AlliancesApplications(self.sr.wndViewParent)
    elif tabName == 'alliances_members':
        self.sr.currentView = form.AlliancesMembers(self.sr.wndViewParent)
    elif tabName == 'alliances_relationships':
        return
    elif tabName == 'relationships_nap':
        self.sr.currentView = form.AlliancesRelationships(self.sr.wndViewParent)
        self.sr.currentView.SetType(const.allianceRelationshipNAP)
    elif tabName == 'relationships_friends':
        self.sr.currentView = form.AlliancesRelationships(self.sr.wndViewParent)
        self.sr.currentView.SetType(const.allianceRelationshipFriend)
    elif tabName == 'relationships_competitors':
        self.sr.currentView = form.AlliancesRelationships(self.sr.wndViewParent)
        self.sr.currentView.SetType(const.allianceRelationshipCompetitor)
    elif tabName == 'relationships_enemies':
        self.sr.currentView = form.AlliancesRelationships(self.sr.wndViewParent)
        self.sr.currentView.SetType(const.allianceRelationshipEnemy)
    self.sr.currentView.CreateWindow()
    self.sr.currentView.state = uix.UI_NORMAL