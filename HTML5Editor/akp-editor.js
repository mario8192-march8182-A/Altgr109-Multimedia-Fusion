/**
 * AkpEngine HTML5 Editor - JavaScript
 * Sistema completo de edição de jogos
 */

class AkpEditor {
    constructor() {
        this.canvas = document.getElementById('canvas');
        this.ctx = this.canvas.getContext('2d');
        this.currentProject = null;
        this.currentScene = null;
        this.objects = [];
        this.selectedObject = null;
        this.history = [];
        this.historyIndex = -1;
        this.currentTool = 'select';
        this.isPlaying = false;
        this.gridEnabled = true;
        this.physicsEnabled = false;
        this.collidersVisible = false;
        this.zoom = 1;
        this.panX = 0;
        this.panY = 0;

        this.setupCanvas();
        this.setupEventListeners();
        this.initializeEditor();
    }

    setupCanvas() {
        this.resizeCanvas();
        window.addEventListener('resize', () => this.resizeCanvas());
    }

    resizeCanvas() {
        const container = this.canvas.parentElement;
        this.canvas.width = container.clientWidth;
        this.canvas.height = container.clientHeight;
        this.render();
    }

    setupEventListeners() {
        // Canvas Events
        this.canvas.addEventListener('mousemove', (e) => this.onCanvasMouseMove(e));
        this.canvas.addEventListener('mousedown', (e) => this.onCanvasMouseDown(e));
        this.canvas.addEventListener('mouseup', (e) => this.onCanvasMouseUp(e));
        this.canvas.addEventListener('click', (e) => this.onCanvasClick(e));
        this.canvas.addEventListener('wheel', (e) => this.onCanvasWheel(e));

        // Keyboard Events
        document.addEventListener('keydown', (e) => this.onKeyDown(e));
        document.addEventListener('keyup', (e) => this.onKeyUp(e));
    }

    initializeEditor() {
        this.createNewProject('Untitled', 1024, 768);
    }

    createNewProject(name, width, height) {
        this.currentProject = {
            name: name,
            width: width,
            height: height,
            scenes: []
        };
        this.createNewScene('MainScene');
        this.showToast(`Projeto "${name}" criado!`);
        this.updateUI();
    }

    createNewScene(name) {
        const scene = {
            name: name,
            objects: [],
            backgroundColor: '#3c3c3c',
            width: this.currentProject.width,
            height: this.currentProject.height
        };
        this.currentProject.scenes.push(scene);
        this.currentScene = scene;
        this.objects = [];
        this.selectedObject = null;
        this.updateUI();
        this.showToast(`Cena "${name}" criada!`);
    }

    addObject(type) {
        const id = Math.random().toString(36).substr(2, 9);
        const obj = {
            id: id,
            type: type,
            name: `${type}_${id}`,
            x: this.canvas.width / 2 - 25,
            y: this.canvas.height / 2 - 25,
            width: 50,
            height: 50,
            rotation: 0,
            scaleX: 1,
            scaleY: 1,
            opacity: 1,
            color: '#ff0000',
            visible: true,
            properties: {}
        };

        if (type === 'sprite') {
            obj.imagePath = '';
            obj.frameWidth = 50;
            obj.frameHeight = 50;
        } else if (type === 'text') {
            obj.text = 'Texto';
            obj.fontSize = 24;
            obj.fontFamily = 'Arial';
            obj.color = '#ffffff';
        } else if (type === 'button') {
            obj.text = 'Botão';
            obj.onClick = '';
        } else if (type === 'shape') {
            obj.shapeType = 'rectangle';
            obj.strokeColor = '#000000';
            obj.strokeWidth = 2;
        }

        this.objects.push(obj);
        this.currentScene.objects.push(obj);
        this.selectObject(obj);
        this.addToHistory(`Adicionado ${type}`);
        this.updateUI();
        this.showToast(`${type} adicionado!`);
    }

    selectObject(obj) {
        this.selectedObject = obj;
        this.updatePropertiesPanel();
        this.render();
    }

    deleteSelectedObject() {
        if (!this.selectedObject) return;

        const index = this.objects.indexOf(this.selectedObject);
        if (index > -1) {
            this.objects.splice(index, 1);
            this.currentScene.objects = this.objects;
            this.selectedObject = null;
            this.addToHistory('Objeto deletado');
            this.updateUI();
            this.showToast('Objeto deletado!');
        }
    }

    addToHistory(action) {
        this.history = this.history.slice(0, this.historyIndex + 1);
        this.history.push({
            action: action,
            state: JSON.parse(JSON.stringify(this.objects))
        });
        this.historyIndex++;
    }

    undo() {
        if (this.historyIndex > 0) {
            this.historyIndex--;
            this.objects = JSON.parse(JSON.stringify(this.history[this.historyIndex].state));
            this.currentScene.objects = this.objects;
            this.updateUI();
            this.showToast('Desfeito');
        }
    }

    redo() {
        if (this.historyIndex < this.history.length - 1) {
            this.historyIndex++;
            this.objects = JSON.parse(JSON.stringify(this.history[this.historyIndex].state));
            this.currentScene.objects = this.objects;
            this.updateUI();
            this.showToast('Refeito');
        }
    }

    onCanvasMouseMove(e) {
        const rect = this.canvas.getBoundingClientRect();
        const x = (e.clientX - rect.left) / this.zoom - this.panX;
        const y = (e.clientY - rect.top) / this.zoom - this.panY;

        document.getElementById('statusCoordinates').textContent = `X: ${Math.round(x)} Y: ${Math.round(y)}`;

        if (this.selectedObject && this.currentTool === 'move') {
            this.selectedObject.x = x - 25;
            this.selectedObject.y = y - 25;
            this.addToHistory(`Movido ${this.selectedObject.name}`);
        }

        this.render();
    }

    onCanvasMouseDown(e) {
        const rect = this.canvas.getBoundingClientRect();
        const x = (e.clientX - rect.left) / this.zoom - this.panX;
        const y = (e.clientY - rect.top) / this.zoom - this.panY;

        if (this.currentTool === 'select') {
            for (let i = this.objects.length - 1; i >= 0; i--) {
                const obj = this.objects[i];
                if (x >= obj.x && x <= obj.x + obj.width &&
                    y >= obj.y && y <= obj.y + obj.height) {
                    this.selectObject(obj);
                    return;
                }
            }
            this.selectedObject = null;
            this.updatePropertiesPanel();
        }
    }

    onCanvasMouseUp(e) {
    }

    onCanvasClick(e) {
    }

    onCanvasWheel(e) {
        e.preventDefault();
        const zoomSpeed = 0.1;
        const newZoom = e.deltaY > 0 ? 
            this.zoom - zoomSpeed : 
            this.zoom + zoomSpeed;
        
        this.zoom = Math.max(0.1, Math.min(newZoom, 5));
        document.getElementById('zoomLevel').value = this.zoom;
        this.render();
    }

    onKeyDown(e) {
        if (e.ctrlKey || e.metaKey) {
            if (e.key === 'z') {
                e.preventDefault();
                this.undo();
            } else if (e.key === 'y') {
                e.preventDefault();
                this.redo();
            } else if (e.key === 's') {
                e.preventDefault();
                this.saveProject();
            }
        } else if (e.key === 'Delete') {
            this.deleteSelectedObject();
        }
    }

    onKeyUp(e) {
    }

    updatePropertiesPanel() {
        const panel = document.getElementById('propertiesPanel');
        
        if (!this.selectedObject) {
            panel.innerHTML = '<div class="property-group"><div class="property-group-title">Nenhum Objeto Selecionado</div></div>';
            return;
        }

        let html = '';
        const obj = this.selectedObject;

        // Transform Properties
        html += `
            <div class="property-group">
                <div class="property-group-title">Transform</div>
                <div class="property-item">
                    <label class="property-label">Nome</label>
                    <input type="text" class="property-input" value="${obj.name}" 
                        onchange="editor.selectedObject.name = this.value; editor.updateUI();">
                </div>
                <div class="property-item">
                    <label class="property-label">X: ${Math.round(obj.x)}</label>
                    <input type="number" class="property-input" value="${Math.round(obj.x)}" 
                        onchange="editor.selectedObject.x = parseInt(this.value); editor.render();">
                </div>
                <div class="property-item">
                    <label class="property-label">Y: ${Math.round(obj.y)}</label>
                    <input type="number" class="property-input" value="${Math.round(obj.y)}" 
                        onchange="editor.selectedObject.y = parseInt(this.value); editor.render();">
                </div>
                <div class="property-item">
                    <label class="property-label">Largura: ${Math.round(obj.width)}</label>
                    <input type="number" class="property-input" value="${Math.round(obj.width)}" 
                        onchange="editor.selectedObject.width = parseInt(this.value); editor.render();">
                </div>
                <div class="property-item">
                    <label class="property-label">Altura: ${Math.round(obj.height)}</label>
                    <input type="number" class="property-input" value="${Math.round(obj.height)}" 
                        onchange="editor.selectedObject.height = parseInt(this.value); editor.render();">
                </div>
                <div class="property-item">
                    <label class="property-label">Rotação: ${Math.round(obj.rotation)}°</label>
                    <input type="number" class="property-input" value="${Math.round(obj.rotation)}" 
                        onchange="editor.selectedObject.rotation = parseInt(this.value); editor.render();">
                </div>
            </div>
        `;

        // Appearance Properties
        html += `
            <div class="property-group">
                <div class="property-group-title">Aparência</div>
                <div class="property-item">
                    <label class="property-label">Cor</label>
                    <input type="color" class="color-input" value="${obj.color}" 
                        onchange="editor.selectedObject.color = this.value; editor.render();">
                </div>
                <div class="property-item">
                    <label class="property-label">Opacidade: ${(obj.opacity * 100).toFixed(0)}%</label>
                    <input type="range" class="property-input" min="0" max="1" step="0.1" value="${obj.opacity}" 
                        onchange="editor.selectedObject.opacity = parseFloat(this.value); editor.render();">
                </div>
                <div class="property-item">
                    <label class="property-label">
                        <input type="checkbox" ${obj.visible ? 'checked' : ''} 
                            onchange="editor.selectedObject.visible = this.checked; editor.render();"> Visível
                    </label>
                </div>
            </div>
        `;

        // Type-specific properties
        if (obj.type === 'text') {
            html += `
                <div class="property-group">
                    <div class="property-group-title">Texto</div>
                    <div class="property-item">
                        <label class="property-label">Conteúdo</label>
                        <input type="text" class="property-input" value="${obj.text}" 
                            onchange="editor.selectedObject.text = this.value; editor.render();">
                    </div>
                    <div class="property-item">
                        <label class="property-label">Tamanho da Fonte</label>
                        <input type="number" class="property-input" value="${obj.fontSize}" 
                            onchange="editor.selectedObject.fontSize = parseInt(this.value); editor.render();">
                    </div>
                </div>
            `;
        }

        if (obj.type === 'shape') {
            html += `
                <div class="property-group">
                    <div class="property-group-title">Forma</div>
                    <div class="property-item">
                        <label class="property-label">Tipo</label>
                        <select class="property-input" onchange="editor.selectedObject.shapeType = this.value; editor.render();">
                            <option ${obj.shapeType === 'rectangle' ? 'selected' : ''}>rectangle</option>
                            <option ${obj.shapeType === 'circle' ? 'selected' : ''}>circle</option>
                            <option ${obj.shapeType === 'triangle' ? 'selected' : ''}>triangle</option>
                        </select>
                    </div>
                </div>
            `;
        }

        panel.innerHTML = html;
    }

    updateUI() {
        // Update scenes list
        const scenesList = document.getElementById('scenesList');
        scenesList.innerHTML = this.currentProject.scenes.map((scene, i) => `
            <div class="sidebar-item ${scene === this.currentScene ? 'active' : ''}" 
                onclick="editor.selectScene(${i})">📄 ${scene.name}</div>
        `).join('');

        // Update hierarchy
        const hierarchyList = document.getElementById('hierarchyList');
        hierarchyList.innerHTML = this.objects.map(obj => `
            <div class="hierarchy-item ${obj === this.selectedObject ? 'selected' : ''}" 
                onclick="editor.selectObject(this.parentElement.dataset.obj, event)">
                <div class="hierarchy-expand">▶</div>
                <span>${obj.name}</span>
            </div>
        `).join('');

        // Attach objects to hierarchy items
        Array.from(hierarchyList.children).forEach((el, i) => {
            el.dataset.obj = i;
        });

        // Update canvas info
        document.getElementById('canvasInfo').textContent = 
            `Cena: ${this.currentScene.name} | Objetos: ${this.objects.length}`;
    }

    selectScene(index) {
        this.currentScene = this.currentProject.scenes[index];
        this.objects = this.currentScene.objects;
        this.selectedObject = null;
        this.updateUI();
    }

    render() {
        // Clear canvas
        this.ctx.fillStyle = this.currentScene.backgroundColor;
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

        // Apply transformations
        this.ctx.save();
        this.ctx.scale(this.zoom, this.zoom);
        this.ctx.translate(this.panX, this.panY);

        // Draw grid
        if (this.gridEnabled) {
            this.drawGrid();
        }

        // Draw scene background
        this.ctx.fillStyle = this.currentScene.backgroundColor;
        this.ctx.fillRect(0, 0, this.currentScene.width, this.currentScene.height);
        this.ctx.strokeStyle = '#666666';
        this.ctx.lineWidth = 2;
        this.ctx.strokeRect(0, 0, this.currentScene.width, this.currentScene.height);

        // Draw objects
        for (const obj of this.objects) {
            if (!obj.visible) continue;
            this.drawObject(obj);
        }

        // Draw selection
        if (this.selectedObject) {
            this.drawSelection(this.selectedObject);
        }

        this.ctx.restore();
    }

    drawGrid() {
        const gridSize = 20;
        this.ctx.strokeStyle = '#444444';
        this.ctx.lineWidth = 0.5;

        for (let x = 0; x < this.currentScene.width; x += gridSize) {
            this.ctx.beginPath();
            this.ctx.moveTo(x, 0);
            this.ctx.lineTo(x, this.currentScene.height);
            this.ctx.stroke();
        }

        for (let y = 0; y < this.currentScene.height; y += gridSize) {
            this.ctx.beginPath();
            this.ctx.moveTo(0, y);
            this.ctx.lineTo(this.currentScene.width, y);
            this.ctx.stroke();
        }
    }

    drawObject(obj) {
        this.ctx.save();
        this.ctx.globalAlpha = obj.opacity;
        this.ctx.translate(obj.x + obj.width / 2, obj.y + obj.height / 2);
        this.ctx.rotate((obj.rotation * Math.PI) / 180);
        this.ctx.translate(-(obj.width / 2), -(obj.height / 2));

        if (obj.type === 'sprite') {
            this.ctx.fillStyle = obj.color;
            this.ctx.fillRect(0, 0, obj.width, obj.height);
        } else if (obj.type === 'text') {
            this.ctx.fillStyle = obj.color;
            this.ctx.font = `${obj.fontSize}px ${obj.fontFamily || 'Arial'}`;
            this.ctx.fillText(obj.text, 10, obj.fontSize);
        } else if (obj.type === 'button') {
            this.ctx.fillStyle = obj.color;
            this.ctx.fillRect(0, 0, obj.width, obj.height);
            this.ctx.fillStyle = '#ffffff';
            this.ctx.font = '14px Arial';
            this.ctx.textAlign = 'center';
            this.ctx.fillText(obj.text, obj.width / 2, obj.height / 2 + 5);
        } else if (obj.type === 'shape') {
            this.ctx.fillStyle = obj.color;
            if (obj.shapeType === 'rectangle') {
                this.ctx.fillRect(0, 0, obj.width, obj.height);
            } else if (obj.shapeType === 'circle') {
                this.ctx.beginPath();
                this.ctx.arc(obj.width / 2, obj.height / 2, Math.min(obj.width, obj.height) / 2, 0, Math.PI * 2);
                this.ctx.fill();
            }
        } else if (obj.type === 'light') {
            const gradient = this.ctx.createRadialGradient(obj.width / 2, obj.height / 2, 0, obj.width / 2, obj.height / 2, Math.max(obj.width, obj.height) / 2);
            gradient.addColorStop(0, obj.color);
            gradient.addColorStop(1, 'rgba(0,0,0,0)');
            this.ctx.fillStyle = gradient;
            this.ctx.fillRect(0, 0, obj.width, obj.height);
        }

        this.ctx.restore();
    }

    drawSelection(obj) {
        this.ctx.save();
        this.ctx.strokeStyle = '#00aaff';
        this.ctx.lineWidth = 2;
        this.ctx.dashedLine = [5, 5];
        this.ctx.setLineDash([5, 5]);

        this.ctx.translate(obj.x + obj.width / 2, obj.y + obj.height / 2);
        this.ctx.rotate((obj.rotation * Math.PI) / 180);
        this.ctx.translate(-(obj.width / 2), -(obj.height / 2));

        this.ctx.strokeRect(0, 0, obj.width, obj.height);

        // Draw corners
        const size = 6;
        this.ctx.fillStyle = '#00aaff';
        this.ctx.fillRect(-size / 2, -size / 2, size, size);
        this.ctx.fillRect(obj.width - size / 2, -size / 2, size, size);
        this.ctx.fillRect(-size / 2, obj.height - size / 2, size, size);
        this.ctx.fillRect(obj.width - size / 2, obj.height - size / 2, size, size);

        this.ctx.restore();
    }

    saveProject() {
        const json = JSON.stringify(this.currentProject, null, 2);
        const blob = new Blob([json], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.currentProject.name}.akp`;
        a.click();
        URL.revokeObjectURL(url);
        this.showToast('Projeto salvo!');
    }

    exportProject() {
        this.showToast('Exportando para APK... (em desenvolvimento)');
    }

    playScene() {
        this.isPlaying = true;
        document.getElementById('statusMessage').textContent = '▶️ Reproduzindo';
        this.showToast('Cena reproduzindo');
    }

    pauseScene() {
        this.isPlaying = false;
        document.getElementById('statusMessage').textContent = '⏸️ Pausado';
        this.showToast('Cena pausada');
    }

    stopScene() {
        this.isPlaying = false;
        document.getElementById('statusMessage').textContent = 'Pronto';
        this.showToast('Cena parada');
    }

    selectTool(tool) {
        this.currentTool = tool;
        document.querySelectorAll('.toolbar-group .secondary').forEach(btn => btn.style.opacity = '0.5');
        event.target.style.opacity = '1';
    }

    toggleGrid() {
        this.gridEnabled = !this.gridEnabled;
        this.render();
    }

    togglePhysics() {
        this.physicsEnabled = !this.physicsEnabled;
        this.showToast(`Física ${this.physicsEnabled ? 'ativada' : 'desativada'}`);
    }

    toggleColliders() {
        this.collidersVisible = !this.collidersVisible;
        this.render();
    }

    zoomCanvas(zoomLevel) {
        this.zoom = parseFloat(zoomLevel);
        this.render();
    }

    showToast(message) {
        const toast = document.createElement('div');
        toast.className = 'toast';
        toast.textContent = message;
        document.body.appendChild(toast);
        setTimeout(() => {
            toast.remove();
        }, 2000);
    }
}

// Global functions for UI
let editor;

function initializeEditor() {
    editor = new AkpEditor();
}

function newProject() {
    document.getElementById('newProjectModal').classList.add('active');
}

function createNewProject() {
    const name = document.getElementById('projectName').value || 'Untitled';
    const width = parseInt(document.getElementById('projectWidth').value) || 1024;
    const height = parseInt(document.getElementById('projectHeight').value) || 768;
    editor.createNewProject(name, width, height);
    closeModal('newProjectModal');
}

function newScene() {
    document.getElementById('newSceneModal').classList.add('active');
}

function createNewScene() {
    const name = document.getElementById('sceneName').value || 'Cena 1';
    editor.createNewScene(name);
    closeModal('newSceneModal');
}

function addObject(type) {
    editor.addObject(type);
}

function openProject() {
    editor.showToast('Abrir projeto (em desenvolvimento)');
}

function saveProject() {
    editor.saveProject();
}

function exportProject() {
    editor.exportProject();
}

function undo() {
    editor.undo();
}

function redo() {
    editor.redo();
}

function selectTool(tool) {
    editor.selectTool(tool);
}

function playScene() {
    editor.playScene();
}

function pauseScene() {
    editor.pauseScene();
}

function stopScene() {
    editor.stopScene();
}

function toggleGrid() {
    editor.toggleGrid();
}

function togglePhysics() {
    editor.togglePhysics();
}

function toggleColliders() {
    editor.toggleColliders();
}

function zoomCanvas(zoomLevel) {
    editor.zoomCanvas(zoomLevel);
}

function switchTab(tab) {
    document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
    event.target.classList.add('active');
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('active');
}

function selectObject(index) {
    if (index !== undefined) {
        editor.selectObject(editor.objects[index]);
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', initializeEditor);
