/** @type {number?} */
var _requestId = null;

var _csElement;

/** @type {HTMLCanvasElement?} */
var _canvas;

/**
 * @param {HTMLElement} canvasContainer
 * @param {any} csElement
 */
export function start(canvasContainer, csElement) {
    reloadCanvas(canvasContainer);
    _csElement = csElement;
    _requestId = requestAnimationFrame(render);
}

export function reloadCanvas(canvasContainer) {
    _canvas = canvasContainer.querySelector("canvas");
}

async function render() {
    /** @type {number[]} */
    const numbers = await _csElement.invokeMethodAsync("JsGetBars");

    const ctx = _canvas.getContext("2d");

    ctx.fillStyle = "blue";
    ctx.fillRect(0, 0, _canvas.width, _canvas.height);

    ctx.fillStyle = "red";
    const w = _canvas.width / numbers.length; 
    for (let i = 0; i < numbers.length; i++) {
        let x = i * w;
        let h = numbers[i] * _canvas.height;
        ctx.fillRect(x, _canvas.height - h, w, h);
    }

    // request id is null when we want to stop
    if (_requestId) {
        _requestId = requestAnimationFrame(render);
    }
}

export function stop() {
    cancelAnimationFrame(_requestId)
    _requestId = null;
}