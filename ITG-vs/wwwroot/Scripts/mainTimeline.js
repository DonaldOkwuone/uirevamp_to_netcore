/*
    Variables used to control spacing of descriptions / bars
*/
var origLen = yBar = barHgt = yText = 350,
    stepDiff = 70;
/*
    Timeline SVG dimensions
*/
var div = document.getElementById("content");
/*
    Width of textblocks
*/
var textWidth = 150;
/*
    Block offset
*/
var offset = navigator.userAgent.indexOf("Chrome") == -1 ? 50 : 25;
/*
    Variables that will be referenced throughout initialization
    and rendering
*/
var div, realW, realH, margin = {}, w, h, xmlDoc, svg, dataset = [],
    minDate, maxDate, mainScale, mainAxis, desc, descBox,
    bars, miniRects, boxWidth;
/*
    Checks current clientWidth and adjusts dimension fields accordingly
*/
function updateDimensions() {

    realW = div.clientWidth,
    realH = 750,
    margin = { top: 100, right: (realW / 5), bottom: 100, left: (realW / 5) },
    w = realW - margin.right - margin.left,
    h = realH - margin.top - margin.bottom;
    boxWidth = 155;
}
/*
    initTimeline generates a new timeline every time data in the web form is
    updated
*/
function initTimeline() {
    updateDimensions();

    /*
        access date information and put it in appropriate form 
    */
    dataset = [];
    for (i = 0; i < xmlDoc.length - 1; i++) {
        dataset[i] = xmlDoc[i].split('=');
    }
    for (i = 0; i < dataset.length; i++) {
        temp = dataset[i][0];
        dataset[i][0] = new Date(dataset[i][1]);
        dataset[i][1] = temp;
    }
    /*
        Initialize parameters for main axis
    */
    minDate = dataset[0][0];
    maxDate = dataset[dataset.length - 1][0];
    mainScale = d3.scaleTime()
        .domain([minDate, maxDate]);        

    svg = d3.select("#d3Timeline")
        .append("svg")
        .attr("id", "main");

    mainAxis = d3.axisBottom(mainScale)
        .ticks(dataset.length);

    svg.append("g")
        .attr("class", "axis")
        .attr("id", "mainAxis");
    /*
        Render timeline content
    */
    renderTimelineContent();
}
/*
    Renders all of main timeline, renderTimelineContent is
    called by initTimeline as well as redraw, which handles resizing
*/
function renderTimelineContent() {
    /*
        Ensure dimensions are up to date, render new axes,
        reset dimensions of main timeline SVGS,
        and update range/extent of axes
    */
    updateDimensions();
    svg.attr("width", w + margin.right + margin.left)
        .attr("height", h + margin.top + margin.bottom);

    mainScale.range([0, realW]);
    setMainAxis();
    /*
        Render Text / Date
    */
    desc = svg.append("g")
        .attr("id", "desc")
        .attr("class", "desc")
        .selectAll("text")
        .data(dataset)
        .enter()
        .append("text")
        .text(function(d) { return d[1] + ": " + d[2]; })
        .attr("x", function(d, i) { return textSetX(d, i); })
        .attr("y", function (d, i) { return textGetY(d, i); })
        .attr("dy", 0)
        .call(wrap, textWidth);
    /*
        Render rects to surround text in the timeline
    */
    textCoordsBox = [];
    genTextCoordsBox();
    descBox = svg.append("g")
        .attr("id", "descBox")
        .selectAll("rect")
        .data(textCoordsBox)
        .enter()
        .append("rect")
        .attr("id", "descRect")
        .attr("x", function (d) { return d.getAttribute("x"); })
        .attr("y", function (d) { return d.getAttribute("y") - 25; })
        .attr("width", boxWidth)
        .attr("height", function (d) {
            return (d.getBoundingClientRect().height + offset);                        
        })
        .style("fill", "gray")
        .style("fill-opacity", 0.1)
        .attr("rx", 1.5)
        .attr("ry", 1.5);
    /*
         Render SVG Rectanges Leading from X-Axis to
         Center of Task Labels
    */
    bars = svg.append("g")
        .attr("id", "bars")
        .selectAll("rect")
        .data(dataset)
        .enter()
        .append("rect")
        .attr("x", function (d, i) { return barsSetX(d, i); })
        .attr("width", 2)
        .attr("y", function (d, i) { return barsGetY(d, i); })
        .attr("height", function (d, i) { return barsGetHeight(d, i); })
        .style("fill", "gray")
        .style("opacity", 0.2);
    /*
        Handle Mouse-hover / mouse-out events for rects - hovering cursor
        over a rect should cause it to darken
    */
    d3.selectAll("#descRect")
        .on("mouseover", hover)
        .on("mouseout", mouseOut);
}
/*
    What follow are helper functions used to compute size and coordinate 
    attributes for rendering text, boxes, bars, etc.
*/
function textSetX(d, i) {
    var xCoord = 0;
    if (i >= 0 && i < 8) {
        return xCoord = mainScale(d[0]);
    } else if (i <= dataset.length - 1 && i >= dataset.length - 3) {
        return mainScale(d[0]) - textWidth;
    } else {
        return mainScale(d[0]) - textWidth / 2;
    }
}

function textGetY(d, i) {
    if (i % 2 == 0) {
        if (yText == stepDiff) {
            yText = origLen;
        }
        return (realH / 2) - yText;
    } else {
        temp = yText;
        yText = yText - stepDiff;
        return (realH / 2) + temp;
    }
}

function barsSetX(d, i) {
    if (i == dataset.length - 1) {
        return mainScale(d[0]) - 2;
    } else {
        return mainScale(d[0]);
    }
}

function barsGetY(d, i) {
    if (i % 2 == 0) {
        if (yBar == stepDiff) {
            yBar = origLen;
        }
        return (realH / 2) - yBar + 25;
    } else {
        yBar = yBar - stepDiff;
        return (realH / 2);
    }
}

function barsGetHeight(d, i) {
    if (i % 2 == 0) {
        if (barHgt == stepDiff) {
            barHgt = origLen;
        }
        return barHgt - 25;
    } else {
        temp = barHgt;
        barHgt = barHgt - stepDiff;
        return temp - 25;
    }
}
/*
    genTextCoordsBox accesses the coordinates of all of the tspan elements
    surrounding wrapped text and adds them to the textCoordsBox list. This list
    is used to determine where the rects surrounding text need to be placed
*/
function genTextCoordsBox() {
    textCoordsBox = [];
    var doc = document.getElementsByClassName("descText");
    for (i = 0; i < doc.length; i++) {
        if (doc[i].firstChild.id == "textSpan") {
            textCoordsBox.push(doc[i]);
        }
    }
}
/*        
    wrap wraps text given the textwidth constraint defined above (canonical 
    implementation of text-wrapping in d3.js, due to Mike Bostock)
*/
function wrap(text, width) {
    text.each(function () {
        var text = d3.select(this),
            words = text.text().split(/\s+/).reverse(),
            wordslen = words.length,
            word,
            line = [],
            lineNumber = 0,
            lineHeight = 1.1,
            y = text.attr("y"),
            x = text.attr("x"),
            dy = parseFloat(text.attr("dy")),
            tspan = text.text(null).append("tspan").attr("x", x).attr("y", y).attr("dy", dy + "em").attr("id", "textSpan");
        while (word = words.pop()) {
            line.push(word);
            tspan.text(line.join(" "));
            if (tspan.node().getComputedTextLength() > width) {
                line.pop();
                tspan.text(line.join(" "));
                line = [word];
                tspan = text.append("tspan").attr("x", x).attr("y", y).attr("dy", ++lineNumber * lineHeight + dy + "em").text(word).attr("id", "textSpan");
            }
        }
        text.attr("class", "descText");
    });
}
/*
    Sets / resets main time-axis
*/
function setMainAxis() {
    svg.select("#mainAxis")
        .attr("transform", "translate(0," + realH / 2 + ")")
        .call(mainAxis)
        .selectAll("text")
        .attr("y", 0)
        .attr("x", 9)
        .attr("dy", "1.2em")
        .attr("transform", "rotate(90)")
        .style("text-anchor", "start");
}

/*
    Event handlers for mouse-over events
*/
function hover(hoverD) {
    d3.selectAll("#descRect")
        .filter(function (d) { return d == hoverD; })
        .style("fill-opacity", 0.2);
}

function mouseOut() {
    d3.selectAll("#descRect")
        .style("fill", "gray")       
        .style("fill-opacity", 0.1);
}
/*
    Clears content from timeline, subequently re-renders
    with a call to renderTimelineContent
*/
function redraw() {
    d3.select("#main").selectAll("rect").remove();
    d3.select("#main").selectAll(".descText").remove();
    renderTimelineContent();
}

/*
    Event listener for resize events, calls redraw which    
*/
window.addEventListener("resize", redraw);
