window.getElementRect = (element) => {
    const r = element.getBoundingClientRect();
    return { top: r.top, height: r.height };
};

window.getScrollLeft = (el) => el.scrollLeft;
window.getClientWidth = (el) => el.clientWidth;