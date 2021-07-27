def Startup(self, *args):
    def ApplyCorporation(*args_):
        def CheckApplication(retval):
            if retval.has_key('appltext'):
                applicationText = retval['appltext']
                if len(applicationText) > 1000:
                    return mls.UI_CORP_HINT4 % {'len': len(applicationText)}
            return ''
        corpid = self.corporationID
        try:
            if eve.session.corpid == corpid:
                raise UserError('CanNotJoinCorpAlreadyAMember', {'corporation': cfg.eveowners.Get(eve.session.corpid).name})
            isCEO = sm.GetService('corp').UserIsCEO()
            if isCEO:
                raise UserError('CeoCanNotJoinACorp', {'CEOsCorporation': cfg.eveowners.Get(eve.session.corpid).name, 'otherCorporation': cfg.eveowners.Get(corpid).name})
            applications = sm.GetService('corp').GetMyApplications(corpid)
            if len(applications) != 0:
                sm.GetService('corpui').ViewApplication(corpid)
                return
            corpName = cfg.eveowners.Get(corpid).name
            tax = sm.GetService('corp').GetCorporation(corpid).taxRate * 100
            format = [{'type': 'header', 'text': mls.UI_STATION_APPLYFORMEMBERSHIPTO % {'corp': corpName}, 'frame': 1}, {'type': 'push'}, {'type': 'btline'}, {'type': 'textedit', 'key': 'appltext', 'label': mls.UI_CORP_APPLICATIONTEXT, 'frame': 1, 'maxLength': 1000}, {'type': 'btline'}, {'type': 'push'}, {'type': 'header', 'text': mls.UI_STATION_CURRENTTAXRATEFOR % {'corp': '<b>%s</b>' % corpName, 'taxrate': tax}, 'frame': 1}]
            format.append({'type': 'errorcheck', 'errorcheck': CheckApplication})
            retval = uix.HybridWnd(format, mls.UI_STATION_JOINCORPORATION, 1, None, uix.OKCANCEL, None, 400, 200, icon='07_06', blockconfirm=True, unresizeAble=True)
            if retval is not None:
                uthread.new(sm.GetService('corp').InsertApplication, corpid, retval['appltext'])
        finally:
            pass

        return

    listentry.Generic.Startup(self, *args)
    self.sr.infoicon = uix.GetInfoLink(None, None, size=16, left=0, top=2, where=self, idx=0)
    self.sr.infoicon.anchors = uix.UI_ANCHRIGHT
    self.sr.infoicon.OnClick = self.ShowInfo
    self.sr.corpicon = uix.GetContainer(where=self, width=32, height=32, left=4, top=1, align=uix.UI_ALNONE)
    self.joinleaveBtn = uix.GetButton(self, mls.UI_CMD_JOIN, left=20, func=ApplyCorporation, idx=0)
    self.joinleaveBtn.autoPos = uix.AUTOPOSYCENTER
    self.joinleaveBtn.anchors = uix.UI_ANCHRIGHT
    self.id = 'test'
    return
