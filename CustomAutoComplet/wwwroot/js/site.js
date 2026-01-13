window.scrollColumnIntoView = (columnId, containerId) => {
    const el = document.getElementById(columnId);
    const container = document.getElementById(containerId);
    if (!el || !container) return;

    // 计算相对于容器左边的位置
    const elRect = el.getBoundingClientRect();
    const containerRect = container.getBoundingClientRect();
    const scrollLeft = container.scrollLeft + (elRect.left - containerRect.left);

    // 平滑滚动到容器中间位置
    container.scrollTo({
        left: scrollLeft - containerRect.width / 2 + elRect.width / 2,
        behavior: 'smooth'
    });
};
