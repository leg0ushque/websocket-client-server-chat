const SENT = "sent", RECEIVED  = "received";

let socket = new WebSocket("ws://localhost:5000/ws");
const messages = document.getElementById("messages");

socket.onmessage = function(event) {
    let messageToAdd = createMessageElement(RECEIVED, event.data, getCurrentTime())
	messages.appendChild(messageToAdd);
	messages.scrollTop = messages.scrollHeight; // Auto-scroll to new message
};

function sendMessage() {
    let messageInput = document.getElementById("message-input");
    socket.send(messageInput.value);
    
    let messageToAdd = createMessageElement(SENT, messageInput.value, getCurrentTime())
	messages.appendChild(messageToAdd);
	messages.scrollTop = messages.scrollHeight;

    messageInput.value = '';
}

document.getElementById('chat-form').addEventListener('submit', function(event) {
    event.preventDefault();  // Prevent the form from submitting in the traditional way.
    sendMessage();
});

socket.onclose = function(event) {
    if (event.wasClean) {
        alert(`Соединение закрыто чисто, код=${event.code}, причина=${event.reason}`);
    } else {
        alert('Соединение прервано');
    }
};

socket.onerror = function(error) {
    alert(`[Ошибка] ${error.message}`);
	debugger;
};

function createMessageElement(messageType, text, time) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message ' + messageType;
    messageDiv.textContent = text;

    // Создание вложенного span для метаданных
    const metadataSpan = document.createElement('span');
    metadataSpan.className = 'metadata';

    // Создание вложенного span для времени
    const timeSpan = document.createElement('span');
    timeSpan.className = 'time';
    timeSpan.textContent = time;

    // Встраиваем timeSpan в metadataSpan
    metadataSpan.appendChild(timeSpan);

    // Встраиваем metadataSpan в messageDiv
    messageDiv.appendChild(metadataSpan);

    return messageDiv;
}


function getCurrentTime() {
    const now = new Date();
    let hours = now.getHours();
    let minutes = now.getMinutes();

    // нули в 08:07
    hours = hours < 10 ? '0' + hours : hours;
    minutes = minutes < 10 ? '0' + minutes : minutes;

    return `${hours}:${minutes}`;
}