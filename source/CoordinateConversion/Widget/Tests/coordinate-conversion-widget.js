///////////////////////////////////////////////////////////////////////////
// Copyright (c) 2016 Esri. All Rights Reserved.
//
// Licensed under the Apache License Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
///////////////////////////////////////////////////////////////////////////

define([
    'dojo/_base/declare',
    'dojo/_base/lang',
    'dojo/on',
    'dojo/topic',
    'dojo/dom-attr',
    'dijit/_WidgetBase',
    'dijit/_TemplatedMixin',
    'dijit/_WidgetsInTemplateMixin',
    'cc/Widget',
    'dojo/text!cc/Widget.html',
    'esri/layers/GraphicsLayer',
    'esri/renderers/SimpleRenderer',
    'esri/symbols/PictureMarkerSymbol'    
], function (
    dojoDeclare,
    dojoLang,
    dojoOn,
    dojoTopic,
    dojoDomAttr,
    dijitWidgetBase,
    dijitTemplatedMixin,
    dijitWidgetsInTemplate,
    CoordConversionWidget,
    CoordConversionTemplate,
    EsriGraphicsLayer,
    EsriSimpleRenderer,
    EsriPictureMarkerSymbol    
) {
    return dojoDeclare([dijitWidgetBase, dijitTemplatedMixin, dijitWidgetsInTemplate], {
        templateString: CoordConversionTemplate,
        config: null,
        map: null,

        postCreate: function () {
            this.coordTypes = [
              'DD',
              'DDM',
              'DMS',
              // 'GARS',
              'MGRS',
              'USNG',
              'UTM'
            ];

            if (this.config.coordinateconversion.initialCoords &&
              this.config.coordinateconversion.initialCoords.length > 0) {
                this.coordTypes = this.config.coordinateconversion.initialCoords;
            }

            // Create graphics layer
            if (!this.coordGLayer) {
                var glsym = new EsriPictureMarkerSymbol(
                    this.folderUrl + 'images/CoordinateLocation.png',
                    26,
                    26
                );
                glsym.setOffset(0, 13);

                var glrenderer = new EsriSimpleRenderer(glsym);

                this.coordGLayer = new EsriGraphicsLayer();
                this.coordGLayer.setRenderer(glrenderer);
                this.map.addLayer(this.coordGLayer);
            }
        },

        startup: function () {
            this.inputControl = new CoordConversionWidget({
                config: this.config,
                map: this.map,
                parentWidget: this,
                input: true,
                type: 'DD'
                folderUrl: this.folderUrl
            });
            this.inputControl.placeAt(this.inputcoordcontainer);
            this.inputControl.startup();

            // add default output coordinates
            dojoArray.forEach(this.coordTypes, function (itm) {
                this.addOutputSrBtn(itm);
            }, this);
        }                        
    });
});
