def Show():
    import sys, blue, uix, triui, xtriui, listentry, spiffy, form, draw, util, uthread, log, pprint
    from service import Service

    graphicsWnd = None
    descriptionIndex = cfg.evegraphics.Get(1).header.index('description')

    def HydrateLine(row):
        # there's some icons that contain a full URL for whatever reason, so take care of those too
        icon = row.icon.strip()

        # there's some specific icons that have wrong information, ignore them
        # these IDs come from the apoc database
        if row.graphicID == 2998 or row.graphicID == 3373:
            raise Exception()

        if icon.startswith('res:/'):
            start = icon.rfind ('/icon') + len('/icon')
            end = icon.rfind ('.blue')

            if end:
                icon = icon[start:end]
            else:
                icon = icon[start:]

        return listentry.Get('IconEntry', {'line': 1, 'hint': '', 'text': '', 'label': str(row.graphicID) + ' - ' + str(row.line[descriptionIndex]), 'icon': icon, 'selectable': 0, 'iconoffset': 0, 'iconsize': 64, 'linecolor': (1.0, 1.0, 1.0, 0.125)})


    def HydrateContent(origin):
        stuff = []

        for row in origin:
            if row.icon and len(row.icon) > 0:
                try:
                    stuff.append(HydrateLine(row))
                except:
                    pass

        return stuff

    def GetContent(node, newitems=0):
        return HydrateContent (cfg.evegraphics)

    def Menu(node, *args):
        # right click does nothing for now
        return [None]

    def Load(*args):
        pass

    def Search(*args):
        scroll = graphicsWnd.sr.scroll
        scroll.sr.id = 'searchreturns'
        searchText = graphicsWnd.sr.input.GetValue().lower()
        stuff = []
        scroll.Load(contentList=[])
        scroll.ShowHint(mls.UI_GENERIC_SEARCHING)

        if not searchText:
            scroll.Load(contentList=[listentry.Get('Generic', {'label': mls.UI_MARKET_PLEASETYPEANDSEARCH})])
            return
        elif searchText.isdigit() is True:
            search = int(searchText)
        else:
            search = searchText

        if descriptionIndex == -1:
            scroll.Load(contentList=[listentry.Get('Generic', {'label': 'Description is not present, search cannot be performed'})])
            return

        for rec in cfg.evegraphics:
            row = cfg.evegraphics.Get(rec.graphicID)
            # the index for descriptions has to be used instead of going through __getattr__ because CCP, in their infinite wisdom
            # overrode it, so name and description all return the graphic's icon value...
            if rec.graphicID == search or rec.line[descriptionIndex].lower().find(search) != -1:
                if rec.icon and len(rec.icon) > 0:
                    try:
                        stuff.append(HydrateLine(rec))
                    except:
                        pass
                    blue.pyos.BeNice()

        if len(stuff) == 0:
            stuff = [
             listentry.Get('Generic', {'label': mls.UI_MARKET_NOTHINGFOUNDWITHSEARCH % {'search': searchText}})]

        scroll.ShowHint()
        scroll.Load(32, stuff, noContentHint='No graphics found')

    def Hide():
        if graphicsWnd:
            graphicsWnd.SelfDestruct()
            graphicsWnd = None
        return
    
    graphicsWnd = wnd = sm.GetService('window').GetWindow('graphicsdb')
    if wnd:
        graphicsWnd.Maximize()
        return
    else:
        graphicsWnd = wnd = sm.GetService('window').GetWindow('graphicsdb', create=1)
        wnd.SetWndIcon(None)
        wnd.SetMinSize([350, 270])
        wnd.SetTopparentHeight(0)
        wnd.SetCaption('Icon Browser')
        mainpar = uix.GetChild(wnd, 'main')
        wnd.sr.tabs = form.TabGroup(uix.GetContainer('tabsparent', where=mainpar))
        main = uix.GetContainer('main', where=mainpar, left=uix.Border(), top=uix.Border())
        body = uix.GetContainer('body', where=main, align=uix.UI_ALCLIENT)
        wnd.sr.browser = xtriui.Scroll(uix.GetContainer('scroll', where=body, align=uix.UI_ALCLIENT))
        wnd.sr.browser.state = uix.UI_PICKCHILDREN
        wnd.sr.browser.Startup()
        searchParent = uix.GetContainer('search', where=body, align=uix.UI_ALCLIENT)
        searchTop = uix.GetContainer('search', where=searchParent, height=25, align=uix.UI_ALTOP)
        btn = uix.GetButton(searchTop, 'Search', func=Search, align=uix.UI_ALRIGHT)
        wnd.sr.input = uix.GetEdit('Search', searchTop, width=-1, left=1, align=uix.UI_ALCLIENT)
        uix.GetContainer('div', where=searchParent, height=5, align=uix.UI_ALTOP)
        wnd.sr.input.OnReturn = Search
        wnd.sr.scroll = xtriui.Scroll(uix.GetContainer('scroll', where=searchParent, align=uix.UI_ALCLIENT))
        wnd.sr.scroll.state = uix.UI_PICKCHILDREN
        wnd.sr.scroll.Startup()
        wnd.sr.tabs.Startup([
         [
          'Browse', wnd.sr.browser, None, 0],
         [
          'Search', searchParent, None, 1]], 'iconbrowsertabs')
        Search()
        stuff = GetContent(None, False)
        wnd.sr.browser.Load(32, stuff, noContentHint='No graphics found')
    return

Show()