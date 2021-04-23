def ShowMessage(self, messageID, attachments=[]):
    wnd = self.GetWnd()
    if wnd is None or wnd.destroyed:
        return
    (msg, attachments) = self.GetMessage(messageID)
    wnd.sr.viewmessageform.parent.state = uix.UI_PICKCHILDREN
    wnd.sr.viewmessageform.SetValue('<h1>' + msg.subject + '</h1><br>' + util.FmtDate(msg.created, 'ls') + '<br>' + msg.body, scrolltotop=1, preformatted=1)
    uix.Flush(wnd.sr.attachments)
    wnd.sr.attachments.state = uix.UI_HIDDEN
    self.showing_message = messageID
    self.AddRead(messageID, msg.senderID)
    self.CheckButtons(msg.senderID)
    settings.char.ui.Set('inboxlastmsg', messageID)
    return
