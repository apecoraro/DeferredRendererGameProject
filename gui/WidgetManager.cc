#include "libopenscenegui.h"

#include <float.h>
#include <vector>
#include <algorithm>

typedef std::vector<osgui::WidgetManager*> WidgetManagerList;

static WidgetManagerList s_widgetManagers;

struct SortByPriority
{
    bool operator()(const osgui::WidgetManager* rhs, const osgui::WidgetManager* lhs)
    {
        return rhs->getPriority() < lhs->getPriority();
    }

    bool operator()(const osgui::Widget* rhs, const osgui::Widget* lhs)
    {
        return rhs->getPriority() < lhs->getPriority();
    }
};

bool osgui::Events::_Model3DEditEventInProgress = false;

bool osgui::Events::InjectEvent(int32 winWidth,
                                int32 winHeight,
                                const osgui::Event& event)
{
    std::sort(s_widgetManagers.begin(), s_widgetManagers.end(), SortByPriority());

    bool eventHandled = false;

    

    WidgetManager* pWidgetManager;

    

    for(WidgetManagerList::iterator itr = s_widgetManagers.begin();
        itr != s_widgetManagers.end();
        ++itr)
    {
        pWidgetManager = *itr;
    
        osgui::Event widgetManagerEvent = pWidgetManager->convertCoords(winWidth, 
                                                        winHeight, 
                                                        event);

        //if(pWidgetManager->isInside(widgetManagerEvent.getX(), 
        //                    widgetManagerEvent.getY()))
        {
            eventHandled = pWidgetManager->handleEvent(widgetManagerEvent);
            if(eventHandled)
                break;
        }
    }

    return eventHandled;
}

bool osgui::Events::InjectKeyboardEvent(const osgui::KeyboardEvent& event)
{
    std::sort(s_widgetManagers.begin(), s_widgetManagers.end(), SortByPriority());

    bool eventHandled = false;

    WidgetManager* pWidgetManager;

    for(WidgetManagerList::iterator itr = s_widgetManagers.begin();
        itr != s_widgetManagers.end();
        ++itr)
    {
         pWidgetManager = *itr;
    
        //if(pWidgetManager->isInside(widgetManagerEvent.getX(), 
        //                    widgetManagerEvent.getY()))
        {
            eventHandled = pWidgetManager->handleEvent(event);
            if(eventHandled)
                break;
        }
    }

    return eventHandled;
}

osgui::WidgetManager::WidgetManager(uint8 defaultWidgetPriority) : 
    _pFocusWidget(NULL),
    //this is the priority of relative to other WidgetManagers
    _priority(0),
    //this is the default priority given to widgets associated with the manager
    _defaultWidgetPriority(defaultWidgetPriority),
    _active(false),
    _needsUpdate(true)
{
    _status = osi_new(OSOM_DRAWING_PAD_CLASS, &_osiID);

    _coordinateSystem.x_min = FLT_MAX;
    _coordinateSystem.y_min = FLT_MAX;
    _coordinateSystem.x_max = -FLT_MAX;
    _coordinateSystem.y_max = -FLT_MAX;

    s_widgetManagers.push_back(this);
}

osgui::WidgetManager::~WidgetManager()
{

    WidgetManager* pWidgetManager;

    for(WidgetManagerList::iterator itr = s_widgetManagers.begin();
        itr != s_widgetManagers.end();
        ++itr)
    {
        pWidgetManager = *itr;
        if(pWidgetManager == this)
        {
            s_widgetManagers.erase(itr);
            break;
        }
    }

    _widgets.clear();
    osi_delete(_osiID);
}

bool osgui::WidgetManager::handleEvent(const osgui::Event& event)
{
    _widgets.sort(SortByPriority());

    // The focus widget is needed for keyboard input.
    // We reset the focus widget whenever the mouse is pressed.
    // That way a widget keeps the focus even when the mouse exits the widget.
    if(event.button1Pressed())
        _pFocusWidget = NULL;

     Widget* pWidget;

    for(WidgetList::iterator itr = _widgets.begin();
        itr != _widgets.end();
        ++itr)
    {
        pWidget = *itr;
        if(!pWidget->getActive() || !pWidget->getEnabled() || pWidget->getClipped())
            continue;

        // The focus widget is the last widget to handle an event.
        // XXX We might need some smarter logic here in case the widget
        // we want to have the focus passes the event down. We already
        // have a case where the widget we want to have the focus is 
        // not the first widget to handle an event, so that's why
        // we set it to be the last widget to handle an event.
        if(pWidget->handleEvent(event))
        {
            _pFocusWidget = pWidget;
            return true;
        }
    }

    return false;
}

bool osgui::WidgetManager::handleEvent(const osgui::KeyboardEvent& event)
{
    if(_pFocusWidget == NULL)
        return false;

    if(!_pFocusWidget->getActive() || !_pFocusWidget->getEnabled() || _pFocusWidget->getClipped())
        return false;

    return _pFocusWidget->handleEvent(event);
}

osgui::Event osgui::WidgetManager::convertCoords(int32 winWidth,
                                         int32 winHeight,
                                         const osgui::Event& windowEvent)
{

    float32 winX = (float32)windowEvent.getX() + _coordinateSystem.x_min;
    float32 winY = (float32)windowEvent.getY() + _coordinateSystem.y_min;
    
    float32 width = _coordinateSystem.x_max - _coordinateSystem.x_min;
    float32 height = _coordinateSystem.y_max - _coordinateSystem.y_min;

    float32 scaleX = width/(float32)winWidth;
    float32 scaleY = height/(float32)winHeight;

    winX *= scaleX;
    winY *= scaleY;
    winY = height - winY;

    osgui::Event widgetManagerEvent(windowEvent);
    widgetManagerEvent.setX((int32)winX);
    widgetManagerEvent.setY((int32)winY);

    return widgetManagerEvent;
}

void osgui::WidgetManager::reverseConvertCoords(int32 winWidthPixels, int32 winHeightPixels, 
                                                int32 pixelX, int32 pixelY,
                                                int32& windowX, int32& windowY)
{
    float32 width = _coordinateSystem.x_max - _coordinateSystem.x_min;
    float32 height = _coordinateSystem.y_max - _coordinateSystem.y_min;

    float32 scaleX = width / (float32) winWidthPixels;
    float32 scaleY = height / (float32) winHeightPixels;

    windowX = static_cast<int32>(pixelX * scaleX);
    windowY = static_cast<int32>(pixelY * scaleY);
}

bool osgui::WidgetManager::isInside(float32 x, float32 y) const
{
    if(x < _coordinateSystem.x_min || x > _coordinateSystem.x_max)
        return false;

    if(y < _coordinateSystem.y_min || y > _coordinateSystem.y_max)
        return false;
 
    return true;
}

void osgui::WidgetManager::registerForEvents(Widget& widget)
{
    _widgets.push_back(&widget);
}

void osgui::WidgetManager::unregisterForEvents(Widget& widget)
{
    Widget* pWidget;
    for(WidgetList::iterator itr = _widgets.begin();
        itr != _widgets.end();
        ++itr)
    {
        pWidget = *itr;
        if(pWidget == &widget)
        {
            _widgets.erase(itr);
            break;
        }
    }
}


void osgui::WidgetManager::addAssociatedView(const osi_id& viewID)
{
    _needsUpdate = true;
    osi_status call_status;
    _status = osi_call(viewID,
                       OS_ADD_ASSOCIATED_2D,
                       &call_status, &(_osiID), OSD_NON_BLOCKING);
}

void osgui::WidgetManager::setCoordinateSystem(float32 xmin, 
                                       float32 ymin, 
                                       float32 xmax, 
                                       float32 ymax)
{
    _needsUpdate = true;
    _coordinateSystem.x_min = xmin;
    _coordinateSystem.y_min = ymin;
    _coordinateSystem.x_max = xmax;
    _coordinateSystem.y_max = ymax;

    osi_status call_status;
    _status = osi_call(_osiID,
                       SET_OS_PAD_WINDOW,
                       &call_status, &(_coordinateSystem), OSD_NON_BLOCKING);
}
        
void osgui::WidgetManager::setPriority(uint8 priority)
{
    _needsUpdate = true;
    _priority = priority;
    
    osi_status call_status;
    _status = osi_call(_osiID,
                       SET_OS_PRIORITY,
                       &call_status, &(_priority), 
                       OSD_NON_BLOCKING);
}
        
void osgui::WidgetManager::setActive(bool active)
{
    _needsUpdate = true;
    _active = active;
    osi_status call_status;

    if(active)
    {
        _status = osi_call(_osiID,
                           OS_ACTIVATE,
                           &call_status, &(_osiID), OSD_NON_BLOCKING);
    }
    else
    {
        _status = osi_call(_osiID,
                           OS_DEACTIVATE,
                           &call_status, &(_osiID), OSD_NON_BLOCKING);
    }

}

void osgui::WidgetManager::updateIG()
{
    if(_needsUpdate)
    {
        _needsUpdate = false;
        osi_status call_status;
        _status = osi_call(_osiID, OS_UPDATE_COMPLETE,
                           &call_status, NULL, OSD_NON_BLOCKING);
    }
}

stpick_pick_callback osgui::WidgetManager::setPickCallback(stpick_pick_callback callback)
{
    return stpick_set_pick_callback(callback);
}

void osgui::WidgetManager::pick(int32 stealthId, int32 windowWidth, int32 windowHeight,
                                float32 jstealthX, float32 jstealthY,
                                uint64 request_id)
{
    // have to convert from JStealth coordinates to pixels
    int x = static_cast<int>((jstealthX - _coordinateSystem.x_min) * windowWidth /
      (_coordinateSystem.x_max - _coordinateSystem.x_min));
    int y = static_cast<int>((jstealthY - _coordinateSystem.y_min) * windowHeight /
      (_coordinateSystem.y_max - _coordinateSystem.y_min));

    stpick_pick(stealthId, x, y, request_id);
}

