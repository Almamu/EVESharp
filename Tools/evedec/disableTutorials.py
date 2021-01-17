import __builtin__

def GetTutorials(self):
    eve.Message('CustomNotify', {'notify': 'Disabling tutorials'})
    __builtin__.settings.char.ui.Set('showTutorials', 0)
    eve.Message('CustomNotify', {'notify': 'Tutorials disabled!'})
    return {}
