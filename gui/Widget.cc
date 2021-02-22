#include "libopenscenegui.h"

#include "libopenscenegui_util.h"

#include <float.h>
#include <stdalloc.h>
#include <iostream>

osgui::Widget::Widget(osgui::WidgetManager& widgetManager) : 
    _pWidgetManager(&widgetManager),
    _pParentWidget(NULL),
    _localTransX(0.0f), 
    _localTransY(0.0f),
    _localRotation(0.0f),
    _screenTransX(0.0f), 
    _screenTransY(0.0f),
    _screenRotation(0.0f),
    _width(0.0f),
    _height(0.0f),
    _priority(0),
    _priorityMode(osgui::Widget::INHERIT),
    _customOriginX(0),
    _customOriginY(0),
    _originMode(osgui::Widget::LOWER_LEFT),
    _needsUpdate(true),
    _inside(false),
    _buttonPressed(false),
    _active(false),
    _clipped(false),
    _screenTransformationComputed(false),
    _enabled(true),
    _updateBounds(false),
    _updateUnclippedBounds(true),
    _boundingRect(0.0f, 0.0f, 0.0f, 0.0f)
{
    mName = NULL;
}

osgui::Widget::~Widget()
{
    _pWidgetManager->unregisterForEvents(*this);

    _attachedWidgets.clear();

    if(mName) STDDEALLOC(mName);
}

void osgui::Widget::setName(const char * name)
{
    if(mName) STDDEALLOC(mName);

    if(!name || !name[0])
    {
        mName = NULL;
        return;
    }

    mName = (char*)STDALLOC((1 + strlen(name)) * sizeof(char));

    strcpy(mName, name);
}

const char * osgui::Widget::getName()
{
    return mName;
}

void osgui::Widget::attach(Widget& widget)
{
    widget.setPriority(_priority-1);
    widget.setActive(_active);
    widget.setParentWidget(this);
    widget.setUpdateBounds(true);
    widget.setScreenTransformationComputed(false);
    _attachedWidgets.push_back(&widget);
}

void osgui::Widget::detach(Widget& widget)
{
    osgui::Widget* pFindMe = &widget;
    osgui::Widget::WidgetList::iterator itr = _attachedWidgets.begin();

    osgui::Widget* pWidget;

    for( ; itr != _attachedWidgets.end(); ++itr)
    {
        pWidget = *itr;

        if(pWidget == pFindMe)
        {
            _attachedWidgets.erase(itr);
            pWidget->setParentWidget(NULL);
            break;
        }
    }
}

void osgui::Widget::setActive(bool active)
{
    _active = active;
    osgui::Widget::WidgetList::iterator itr = _attachedWidgets.begin();

    osgui::Widget* pWidget;

    for( ; itr != _attachedWidgets.end(); ++itr)
    {
        pWidget = *itr;
        pWidget->setActive(_active);
    }
}

void osgui::Widget::clipTo(float32 xMin, float32 xMax, float32 yMin, float32 yMax)
{
    const BoundingRect& boundingRect = getBoundingRect();

    bool contained = xMin <= boundingRect.xMin() && xMax >= boundingRect.xMax() &&
                        yMin <= boundingRect.yMin() && yMax >= boundingRect.yMax();
    setClipped(!contained);

    clipAttachedWidgets(xMin, xMax, yMin, yMax);
}

void osgui::Widget::clipAttachedWidgets(float32 xMin, float32 xMax, float32 yMin, float32 yMax)
{
    osgui::Widget::WidgetList::iterator itr = _attachedWidgets.begin();

    osgui::Widget* pWidget;

    for( ; itr != _attachedWidgets.end(); ++itr)
    {
        pWidget = *itr;
        pWidget->clipTo(xMin, xMax, yMin, yMax);
    }

}

void osgui::Widget::setClipped(bool clipped)
{
    _clipped = clipped;
}

bool osgui::Widget::getClipped() const
{
    return _clipped;
}

bool osgui::Widget::getActive() const
{
    return _active;
}

void osgui::Widget::setEnabled(bool enabled)
{
    _enabled = enabled;
    osgui::Widget::WidgetList::iterator itr = _attachedWidgets.begin();

    osgui::Widget* pWidget;

    for( ; itr != _attachedWidgets.end(); ++itr)
    {
        pWidget = *itr;
        pWidget->setEnabled(_enabled);
    }
}

bool osgui::Widget::getEnabled() const
{
    return _enabled;
}

const osgui::WidgetManager& osgui::Widget::getWidgetManager() const
{
    return *_pWidgetManager;
}

osgui::WidgetManager& osgui::Widget::getWidgetManager()
{
    return *_pWidgetManager;
}

void osgui::Widget::computeBoundingRect()
{
    if(!screenTransformationComputed())
        computeScreenTransformation();
    
    float32 newXMin = _screenTransX;
    float32 newYMin = _screenTransY;
    float32 newXMax = _screenTransX + _width;
    float32 newYMax = _screenTransY + _height;
    
    _updateBounds = false;
    if(!FLOATS_EQUAL(_boundingRect.xMin(), newXMin) ||
       !FLOATS_EQUAL(_boundingRect.yMin(), newYMin) ||
       !FLOATS_EQUAL(_boundingRect.xMax(), newXMax) ||
       !FLOATS_EQUAL(_boundingRect.yMax(), newYMax))
    {
        _boundingRect.xMin() = newXMin;
        _boundingRect.yMin() = newYMin;
        _boundingRect.xMax() = newXMax;
        _boundingRect.yMax() = newYMax;
    }

}

void osgui::Widget::computeUnclippedBoundingRect()
{
    if(!screenTransformationComputed())
        computeScreenTransformation();
    
    float32 newXMin = _screenTransX;
    float32 newYMin = _screenTransY;
    float32 newXMax = _screenTransX + _width;
    float32 newYMax = _screenTransY + _height;
    
    _updateUnclippedBounds = false;
    if(!FLOATS_EQUAL(_unclippedBoundingRect.xMin(), newXMin) ||
       !FLOATS_EQUAL(_unclippedBoundingRect.yMin(), newYMin) ||
       !FLOATS_EQUAL(_unclippedBoundingRect.xMax(), newXMax) ||
       !FLOATS_EQUAL(_unclippedBoundingRect.yMax(), newYMax))
    {
        _unclippedBoundingRect.xMin() = newXMin;
        _unclippedBoundingRect.yMin() = newYMin;
        _unclippedBoundingRect.xMax() = newXMax;
        _unclippedBoundingRect.yMax() = newYMax;
    }    
}

void osgui::Widget::setWidthHeight(float32 width, float32 height)
{
    
    if(!FLOATS_EQUAL(_width, width) ||
       !FLOATS_EQUAL(_height, height))
    {
        _width = width;
        _height = height;
        _needsUpdate = true;
        _updateBounds = true;
    }
}



void osgui::Widget::translateBoundingRect(float32 transX, float32 transY) const
{
    _boundingRect.xMin() += transX;
    _boundingRect.yMin() += transY;
    _boundingRect.xMax() += transX;
    _boundingRect.yMax() += transY;
}

void osgui::Widget::setScreenTransformation(float32 x, float32 y, float32 rot) const
{
    if(!FLOATS_EQUAL(x, _screenTransX) || 
       !FLOATS_EQUAL(y, _screenTransY) ||
       !FLOATS_EQUAL(rot, _screenRotation))
    {
        //TODO this should apply the rotation in addition to the translation
        translateBoundingRect(-_screenTransX, -_screenTransY);

        _screenTransX = x;
        _screenTransY = y;
        _screenRotation = rot;

        translateBoundingRect(_screenTransX, _screenTransY);
        
        _needsUpdate = true;
    }
}

void osgui::Widget::setTranslation(float32 x, float32 y)
{
    if(!FLOATS_EQUAL(x, _localTransX) || !FLOATS_EQUAL(y, _localTransY))
    {
        _localTransX = x;
        _localTransY = y;

        _needsUpdate = true;
        _screenTransformationComputed = false;
        _updateUnclippedBounds = true;
        _updateBounds = true;
        osgui::Widget::WidgetList::iterator itr = _attachedWidgets.begin();

        osgui::Widget* pWidget;

        for( ; itr != _attachedWidgets.end(); ++itr)
        {
            pWidget = *itr;
            pWidget->setNeedsUpdate(true);
            pWidget->setScreenTransformationComputed(false);
            pWidget->setUpdateUnclippedBounds(true);
            pWidget->setUpdateBounds(true);
        }
    }
}

void osgui::Widget::setRotation(float32 degrees)
{
    //switch the degrees to negative so that it rotates clockwise
    //with positive values - which seems more intuitive to me
    float32 radians = -degrees * 3.141592f / 180.0f;

    if(!FLOATS_EQUAL(radians, _localRotation))
    {
        _localRotation = radians;

        _needsUpdate = true;
        _screenTransformationComputed = false;
        _updateUnclippedBounds = true;
        _updateBounds = true;
        osgui::Widget::WidgetList::iterator itr = _attachedWidgets.begin();

        osgui::Widget* pWidget;

        for( ; itr != _attachedWidgets.end(); ++itr)
        {
            pWidget = *itr;
            pWidget->setNeedsUpdate(true);
            pWidget->setScreenTransformationComputed(false);
            pWidget->setUpdateUnclippedBounds(true);
            pWidget->setUpdateBounds(true);
        }
    }

}

void osgui::Widget::setNeedsUpdate(bool flag) const
{
    _needsUpdate = flag;
    osgui::Widget::WidgetList::const_iterator itr = _attachedWidgets.begin();

    const osgui::Widget* pWidget;

    for( ; itr != _attachedWidgets.end(); ++itr)
    {
        pWidget = *itr;
        pWidget->setNeedsUpdate(flag);
    }
}

void osgui::Widget::setScreenTransformationComputed(bool flag) const
{
    _screenTransformationComputed = flag;

    osgui::Widget::WidgetList::const_iterator itr = _attachedWidgets.begin();

    const osgui::Widget* pWidget;

    for( ; itr != _attachedWidgets.end(); ++itr)
    {
        pWidget = *itr;
        pWidget->setScreenTransformationComputed(flag);
    }
}

void osgui::Widget::setUpdateBounds(bool flag) const
{
    _updateBounds = flag;
    
    osgui::Widget::WidgetList::const_iterator itr = _attachedWidgets.begin();

    const osgui::Widget* pWidget;

    for( ; itr != _attachedWidgets.end(); ++itr)
    {
        pWidget = *itr;
        pWidget->setUpdateBounds(flag);
    }
}

void osgui::Widget::setUpdateUnclippedBounds(bool flag) const
{
    _updateUnclippedBounds = flag;
    
    osgui::Widget::WidgetList::const_iterator itr = _attachedWidgets.begin();

    const osgui::Widget* pWidget;

    for( ; itr != _attachedWidgets.end(); ++itr)
    {
        pWidget = *itr;
        pWidget->setUpdateUnclippedBounds(flag);
    }
}

void osgui::Widget::addTranslation(float32 x, float32 y)
{
    setTranslation(_localTransX + x, _localTransY + y);
}

void osgui::Widget::setPriority(uint8 priority)
{
    if((uint8)_priority != priority)
    {
        _priority = priority;
        _needsUpdate = true;
        updateAttachedWidgetsPriority();
    }
}

void osgui::Widget::increasePriority(uint8 add/*=1*/)
{
    setPriority(_priority + add);
}

void osgui::Widget::decreasePriority(uint8 sub/*=1*/)
{
    setPriority(_priority - sub);
}

void osgui::Widget::updateAttachedWidgetsPriority()
{
    osgui::Widget::WidgetList::iterator itr = _attachedWidgets.begin();

    osgui::Widget* pWidget;

    for( ; itr != _attachedWidgets.end(); ++itr)
    {
        pWidget = *itr;
        
        if(pWidget->getPriorityMode() == INHERIT)
            pWidget->setPriority((uint8)(_priority)-1);

        pWidget->updateAttachedWidgetsPriority();
    }
}

uint8 osgui::Widget::getHighestPriority() const
{
    uint8 maxPriority = (uint8)_priority;

    osgui::Widget::WidgetList::const_iterator itr = _attachedWidgets.begin();

    osgui::Widget* pWidget;

    for( ; itr != _attachedWidgets.end(); ++itr)
    {
        pWidget = *itr;

        uint8 attachedMax = pWidget->getHighestPriority();
        if(attachedMax > maxPriority)
            maxPriority = attachedMax;
    }

    return maxPriority;
}

bool osgui::Widget::screenTransformationComputed() const
{
    return _screenTransformationComputed;
}

void osgui::Widget::updateIG()
{
    if(!screenTransformationComputed())
        computeScreenTransformation();

    updateAttachedWidgets();
    _needsUpdate = false;
}

void osgui::Widget::getScreenTransformation(float32& xOut, float32& yOut, float32& rotOut)  const
{
    if(!screenTransformationComputed())
        computeScreenTransformation();

    xOut = _screenTransX;
    yOut = _screenTransY;
    rotOut = _screenRotation;
}

void osgui::Widget::computeScreenTransformation() const
{
    float32 parentTransX = 0.0f;
    float32 parentTransY = 0.0f;
    float32 parentRot = 0.0f;
    if(_pParentWidget)
    {
        _pParentWidget->getScreenTransformation(parentTransX, parentTransY, parentRot);
    }

    setScreenTransformation(parentTransX + _localTransX,
                            parentTransY + _localTransY,
                            parentRot + _localRotation);

    _screenTransformationComputed = true;
}

void osgui::Widget::updateAttachedWidgets()
{
    osgui::Widget::WidgetList::iterator itr = _attachedWidgets.begin();

    osgui::Widget* pWidget;

    for( ; itr != _attachedWidgets.end(); ++itr)
    {
        pWidget = *itr;
        
        pWidget->updateIG();
    }
}

bool osgui::Widget::needsUpdate() const
{
    return _needsUpdate;
}

void osgui::Widget::addEventHandler(EventHandler& handler)
{
    _eventHandlers.push_back(&handler);
    handler.onAddEventHandler(*this);
}

void osgui::Widget::insertEventHandler(EventHandler& handler)
{
    _eventHandlers.insert(_eventHandlers.begin(),&handler);
    handler.onAddEventHandler(*this);
}

void osgui::Widget::removeEventHandler(EventHandler& handler)
{
    EventHandlerList::iterator itr = _eventHandlers.begin();

    EventHandler* pHandler;

    for( ; itr != _eventHandlers.end(); ++itr)
    {
        pHandler = *itr;
        if (pHandler == &handler)
        {
            _eventHandlers.erase(itr);
            return;
        }
    }    
}

bool osgui::Widget::isInside(float32 x, float32 y) const
{
    const BoundingRect& boundingRect = getBoundingRect();
    if(x < boundingRect.xMin() || x > boundingRect.xMax())
        return false;

    if(y < boundingRect.yMin() || y > boundingRect.yMax())
        return false;
 
    return true;
}

bool osgui::Widget::handleEvent(const osgui::KeyboardEvent& event)
{
    EventHandlerList::iterator itr = _eventHandlers.begin();

    EventHandler* pHandler;

    for( ; itr != _eventHandlers.end(); ++itr)
    {
        pHandler = *itr;

        if(event.getKeyState() == osgui::KeyboardEvent::DOWN)
        {
            if(pHandler->onKeyDown(event, *this))
                return true;
        }
        else
        {
            if(pHandler->onKeyUp(event, *this))
                return true;
        }
    }

    return false;
}

bool osgui::Widget::handleEvent(const osgui::Event& event)
{
    bool stopPassingEvent = false;

    if(isInside(event.getX(), event.getY()))
    {
        if(!_inside)
        {
            _inside = true;
            stopPassingEvent = handleEnter(event);
        }
        
        if(event.buttonPressed())
        {
            _buttonPressed = true;
            stopPassingEvent = handleButtonPressed(event);
        }
        
        if(event.doubleClick())
        {
          _buttonPressed = false;
            stopPassingEvent = handleDoubleClick(event);
        }

    }
    else if(_inside)
    {
        _inside = false;
        stopPassingEvent = handleExit(event);
    }
    
    if(_buttonPressed && event.buttonReleased())
    {
        _buttonPressed = false;
        stopPassingEvent = handleButtonReleased(event);
    }
    else if(_buttonPressed || _inside)
    {
        stopPassingEvent = handleMove(event);
    }
    
    //return true as long as a button is held down
    return _buttonPressed || stopPassingEvent;
}

bool osgui::Widget::handleEnter(const osgui::Event& event)
{
    EventHandlerList::iterator itr = _eventHandlers.begin();

    EventHandler* pHandler;

    for( ; itr != _eventHandlers.end(); ++itr)
    {
        pHandler = *itr;

        if(pHandler->onEnter(event, *this))
            return true;
    }

    return false;
}

bool osgui::Widget::handleExit(const osgui::Event& event)
{
    EventHandlerList::iterator itr = _eventHandlers.begin();

    EventHandler* pHandler;

    for( ; itr != _eventHandlers.end(); ++itr)
    {
        pHandler = *itr;

        if(pHandler->onExit(event, *this))
            return true;
    }

    return false;
}

bool osgui::Widget::handleButtonReleased(const osgui::Event& event)
{
    EventHandlerList::iterator itr = _eventHandlers.begin();

    EventHandler* pHandler;

    for( ; itr != _eventHandlers.end(); ++itr)
    {
        pHandler = *itr;

        if(pHandler->onButtonReleased(event, *this))
            return true;
    }
    
    return false;
}

bool osgui::Widget::handleButtonPressed(const osgui::Event& event)
{
    EventHandlerList::iterator itr = _eventHandlers.begin();

    EventHandler* pHandler;

    for( ; itr != _eventHandlers.end(); ++itr)
    {
        pHandler = *itr;

        if(pHandler->onButtonPressed(event, *this))
            return true;
    }
    
    return false;
}

bool osgui::Widget::handleMove(const osgui::Event& event)
{
    EventHandlerList::iterator itr = _eventHandlers.begin();

    EventHandler* pHandler;

    for( ; itr != _eventHandlers.end(); ++itr)
    {
        pHandler = *itr;

        if(pHandler->onMove(event, *this))
            return true;
    }

    return false;
}

bool osgui::Widget::handleDoubleClick(const osgui::Event& event)
{
    EventHandlerList::iterator itr = _eventHandlers.begin();

    EventHandler* pHandler;

    for( ; itr != _eventHandlers.end(); ++itr)
    {
        pHandler = *itr;

        if(pHandler->onDoubleClick(event, *this))
            return true;
    }

    return false;
}
