/* global process */
var amqp = require('amqplib');
var basename = require('path').basename;
var all = require('when').all;
var rethinkdb = require('rethinkdb');
var _ = require("lodash");

var rethinkdb_conn;
var ex = 'content_publish';

amqp.connect('amqp://localhost').then(function (conn) {

	process.once('SIGINT', function () { conn.close(); });

	// 	rethinkdb.connect({ host: 'localhost', port: 28015 }, function (err, conn) {
	// 		if (err) throw err;
	// 		rethinkdb_conn = conn;
	// 
	// 		rethinkdb.db('content').tableCreate('item').run(rethinkdb_conn, function (err, result) {
	// 			if (err) throw err;
	// 			console.log(JSON.stringify(result, null, 2));
	// 		})
	// 
	// 	});


	conn.createChannel().then(function (ch) {

		var ok = ch.assertExchange(ex, 'topic', { durable: false });

		ok.then(function () {
			return ch.assertQueue('', { exclusive: true });
		})
			.then(function (qok) {
				var queue = qok.queue;
				ch.bindQueue(queue, ex, 'add.sitecore.content.home.content');
				return queue;
			})
			.then(function (queue) {
				return ch.consume(queue, store('sitecore.content.home.content'), { noAck: true });
			});

	});

	conn.createChannel().then(function (ch) {

		var ok = ch.assertExchange(ex, 'topic', { durable: false });

		ok.then(function () {
			return ch.assertQueue('', { exclusive: true });
		})
			.then(function (qok) {
				var queue = qok.queue;
				ch.bindQueue(queue, ex, 'add.sitecore.content.home.content.*');
				return queue;
			})
			.then(function (queue) {
				return ch.consume(queue, store('sitecore.content.home.content.*'), { noAck: true });
			});

	});

	var flatten = function (array) {
		var list = [];
		_.each(array, function (item) {
			list.concat(flatten(item.Children));
			//delete item.Children;
			list.push(item);
			
		});
		return list
	}


	var store = function (prefix) {
		return function (msg) {

			var content = msg.content.toString();

			var json = JSON.parse(content)

			var items = json.Items;

			console.log(items);
			
			//console.log(flatten(items));
		};
	}

	console.log(' [*] Waiting for logs. To exit press CTRL+C.');
}).then(null, console.warn);