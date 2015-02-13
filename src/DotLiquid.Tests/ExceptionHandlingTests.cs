using System;
using System.IO;
using DotLiquid.Exceptions;
using NUnit.Framework;
using ArgumentException = DotLiquid.Exceptions.ArgumentException;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ExceptionHandlingTests
    {
        private class ExceptionDrop : Drop
        {
            public void ArgumentException()
            {
                throw new ArgumentException("argument exception");
            }

            public void SyntaxException()
            {
                throw new SyntaxException("syntax exception");
            }

            public void InterruptException()
            {
                throw new InterruptException("interrupted");
            }
        }

        private class AggregateExceptionTag : Tag
        {
            public override void Render(Context context, TextWriter result)
            {
                throw new AggregateException("One or more errors.", new[]
		        {
		            new Exception("first error"),
                    new Exception("second error")
		        });
            }
        }

        [Test]
        public void TestSyntaxException()
        {
            Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.syntax_exception }} "); });
            string result = template.Render(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() }));
            Assert.AreEqual(" Liquid syntax error: syntax exception ", result);

            Assert.AreEqual(1, template.Errors.Count);
            Assert.IsInstanceOf<SyntaxException>(template.Errors[0]);
        }

        [Test]
        public void TestArgumentException()
        {
            Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.argument_exception }} "); });
            string result = template.Render(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() }));
            Assert.AreEqual(" Liquid error: argument exception ", result);

            Assert.AreEqual(1, template.Errors.Count);
            Assert.IsInstanceOf<ArgumentException>(template.Errors[0]);
        }

        [Test]
        public void TestMissingEndTagParseTimeError()
        {
            Assert.Throws<SyntaxException>(() => Template.Parse(" {% for a in b %} ... "));
        }

        [Test]
        public void TestUnrecognizedOperator()
        {
            Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {% if 1 =! 2 %}ok{% endif %} "); });
            Assert.AreEqual(" Liquid error: Unknown operator =! ", template.Render());

            Assert.AreEqual(1, template.Errors.Count);
            Assert.IsInstanceOf<ArgumentException>(template.Errors[0]);
        }

        [Test]
        public void TestInterruptException()
        {
            Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.interrupt_exception }} "); });
            var localVariables = Hash.FromAnonymousObject(new { errors = new ExceptionDrop() });
            var exception = Assert.Throws<InterruptException>(() => template.Render(localVariables));

            Assert.AreEqual("interrupted", exception.Message);
        }

        [Test]
        public void TestAggregateException()
        {
            Template template = null;
            var config = TemplateConfiguration.CreateStandartConfiguration().RegisterTag<AggregateExceptionTag>("aggregate_exception");
            Assert.DoesNotThrow(() => { template = Template.Parse(" {% aggregate_exception %} ", config); });
            Assert.AreEqual(" Liquid error: first error Liquid error: second error ", template.Render());
        }
    }
}