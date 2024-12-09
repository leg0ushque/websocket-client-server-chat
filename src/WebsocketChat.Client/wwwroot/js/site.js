const SENT = "sent", RECEIVED = "received", ERROR = "error";

function createMessageElement(messageType, sender, text, time) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message ' + messageType;

    const textSpan = document.createElement('span');
    textSpan.textContent = text;

    const senderSpan = document.createElement('span');
    senderSpan.className = 'sender';
    senderSpan.textContent = sender;

    const metadataSpan = document.createElement('span');
    metadataSpan.className = 'metadata';

    const timeSpan = document.createElement('span');
    timeSpan.className = 'time';
    timeSpan.textContent = time;

    metadataSpan.appendChild(timeSpan);
    if (messageType == RECEIVED) {
        messageDiv.appendChild(senderSpan);
    }

    messageDiv.appendChild(textSpan);

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

function appendMessage(messageType, text, sender = '') {
    let messageToAdd = createMessageElement(
        messageType,
        sender,
        text,
        getCurrentTime())

    messages.appendChild(messageToAdd);
    scrollToLastMessage();
}

function scrollToLastMessage() {
    const messagesContainer = document.querySelector('.conversation-container');
    const lastMessage = messagesContainer.lastElementChild;
    lastMessage.scrollIntoView({ behavior: 'smooth', block: 'end' });
}

function showToast(message) {
    let toast = $('#toast');
    toast.text(message);
    toast.addClass('show');

    setTimeout(function () {
        toast.removeClass('show');
    }, 3000);
}

function createHandshakeMessage(userId, token) {
    return createWebSocketMessage(true, userId, token)
}

function createChatMessage(userId, token, text) {
    return createWebSocketMessage(false, userId, token, text);
}

function createWebSocketMessage(isSystemMessage, userId, token, messageText = '') {
    return {
        isSystemMessage: isSystemMessage,
        userId,
        token,
        messageText
    };
}